﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tcg
{
  static class ActionSet
  {
    public static Func<GameState, GameState> PackAction(GameState state, Delegate action, int[] actualArgs = null, int[] remainArgs = null)
    {
      if (actualArgs == null)
        actualArgs = new int[0];

      if (remainArgs == null)
        remainArgs = new int[0];

      // TODO: extend cases up to 9 arguments
      switch (actualArgs.Length)
      {
        case 0:
          return state => ((SpecifiedAction)action)(state, remainArgs);
        case 1:
          return state => ((SpecifiedAction<int>)action)(state, actualArgs[0], remainArgs);
        case 2:
          return state => ((SpecifiedAction<int, int>)action)(state, actualArgs[0], actualArgs[1], remainArgs);
        case 3:
          return state => ((SpecifiedAction<int, int, int>)action)(state, actualArgs[0], actualArgs[1], actualArgs[2], remainArgs);
        case 4:
          return state => ((SpecifiedAction<int, int, int, int>)action)(state, actualArgs[0], actualArgs[1], actualArgs[2], actualArgs[3], remainArgs);
        case 5:
          return state => ((SpecifiedAction<int, int, int, int, int>)action)(state, actualArgs[0], actualArgs[1], actualArgs[2], actualArgs[3], actualArgs[4], remainArgs);
        case 6:
          return state => ((SpecifiedAction<int, int, int, int, int, int>)action)(state, actualArgs[0], actualArgs[1], actualArgs[2], actualArgs[3], actualArgs[4], actualArgs[5], remainArgs);
        case 7:
          return state => ((SpecifiedAction<int, int, int, int, int, int, int>)action)(state, actualArgs[0], actualArgs[1], actualArgs[2], actualArgs[3], actualArgs[4], actualArgs[5], actualArgs[6], remainArgs);
        case 8:
          return state => ((SpecifiedAction<int, int, int, int, int, int, int, int>)action)(state, actualArgs[0], actualArgs[1], actualArgs[2], actualArgs[3], actualArgs[4], actualArgs[5], actualArgs[6], actualArgs[7], remainArgs);
        case 9:
          return state => ((SpecifiedAction<int, int, int, int, int, int, int, int, int>)action)(state, actualArgs[0], actualArgs[1], actualArgs[2], actualArgs[3], actualArgs[4], actualArgs[5], actualArgs[6], actualArgs[7], actualArgs[8], remainArgs);

        default:
          throw new ArgumentException("Invalid number of args");
      }
    }

    public static Func<GameState, GameState> PackAction(GameState state, ActionType type, int[] actualArgs = null, int[] remainArgs = null)
    {
      return PackAction(state, Actions[type], actualArgs, remainArgs);
    }

    public static GameState PackActionAndExecute(GameState state, ActionType type, int[] actualArgs = null, int[] remainArgs = null)
    {
      return PackAction(state, Actions[type], actualArgs, remainArgs)(state);
    }

    public static GameState PackActionAndExecute(GameState state, Delegate action, int[] actualArgs = null, int[] remainArgs = null)
    {
      return PackAction(state, action, actualArgs, remainArgs)(state);
    }

    public static SpecifiedAction<int, int> Attack = (GameState state, int attackerCardIndex, int targetCardIndex, int[] remainArguments) =>
    {
      var attacker = state.CurrentPlayer;
      var target = state.Players[0].Id != attacker.Id ? state.Players[0] : state.Players[1];

      var attackerCard = attacker.ActiveCards[attackerCardIndex];
      var targetCard = target.ActiveCards[targetCardIndex];

      attackerCard.HP -= targetCard.Attack;
      targetCard.HP -= attackerCard.Attack;

      return state;
    };

    public static SpecifiedAction<int, int, int> Heal = (state, playerIndex, cardIndex, healAmount, remainArguments) =>
    {
      Card card = state.Players[playerIndex].ActiveCards[cardIndex];
      card.HP = Math.Min(card.HP + healAmount, card.MaxHP);

      return state;
    };

    public static SpecifiedAction<int, int, int> DealDamage = (state, playerIndex, cardIndex, damageAmount, remainArguments) =>
    {
      Card card = state.Players[playerIndex].ActiveCards[cardIndex];
      card.HP = card.HP - damageAmount;

      return state;
    };

    public static SpecifiedAction DrawCard = (state, remainArguments) =>
    {
      var cardToTake = state.CurrentPlayer.CardSet[0];
      state.CurrentPlayer.CardSet.RemoveAt(0);
      state.CurrentPlayer.CardsInHand.Add(cardToTake);

      return state;
    };

    public static SpecifiedAction<int> PlayCard = (state, cardIndex, remainArguments) =>
    {
      var cardToDraw = state.CurrentPlayer.CardsInHand[cardIndex];
      if (cardToDraw.ManaCost > state.CurrentPlayer.Hero.Mana)
      {
        throw new ArgumentException("You don't have enough mana");
      }
      state.CurrentPlayer.CardsInHand.RemoveAt(cardIndex);
      state.CurrentPlayer.ActiveCards.Add(cardToDraw);

      state.CurrentPlayer.Hero.Mana -= cardToDraw.ManaCost;

      if (cardToDraw.OnPlayAction != null)
        PackActionAndExecute(state, cardToDraw.OnPlayAction, remainArguments);

      return state;
    };

    public static SpecifiedAction ProcessDeath = (state, _) =>
    {
      foreach (Player p in state.Players)
      {
        List<Card> cardsToRemove = new List<Card>();
        p.ActiveCards.ForEach(card => { if (card.HP <= 0) cardsToRemove.Add(card); });

        foreach (Card c in cardsToRemove)
          p.ActiveCards.Remove(c);
      }

      return state;
    };

    public static SpecifiedAction EndTurn = (state, _) =>
    {
      // only if 2 players
      var notCurrentPlayer = state.Players[0].Id != state.CurrentPlayer.Id ? state.Players[0] : state.Players[1];
      state.CurrentPlayer = notCurrentPlayer;

      return state;
    };

    public static Dictionary<ActionType, Delegate> Actions = new Dictionary<ActionType, Delegate>() {
        {ActionType.Attack, Attack},
        {ActionType.Heal, Heal},
        {ActionType.DrawCard, DrawCard},
        {ActionType.PlayCard, PlayCard},
        {ActionType.ProcessDeath, ProcessDeath},
        {ActionType.DealDamage, DealDamage},
        {ActionType.EndTurn, EndTurn}
      };
  }
}
