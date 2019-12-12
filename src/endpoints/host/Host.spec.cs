using NUnit.Framework;
using tcg;
using System;
using System.Collections.Generic;

namespace tcgTests
{
  [TestFixture]
  public class HostTestsFixture
  {
    private (MiddlewareLocal, MiddlewareLocal, MiddlewareLocal, Host) InitializePreset()
    {
      MiddlewareLocal mid1, mid2, midHost;
      (mid1, mid2, midHost) = MiddlewareLocal.GenerateLinkedMiddlewareLocalTriple();

      Host host = new Host(midHost, new List<MiddlewareLocal> { mid1, mid2 });
      host.state = new GameState(new Player[] { new Player(0, new List<Card>(), new List<Card>(), new List<Card>()), new Player(1, new List<Card>(), new List<Card>(), new List<Card>()) });

      return (mid1, mid2, midHost, host);
    }

    [Test]
    public void Initialize()
    {
      IMiddleware mid1, mid2, midHost;
      Host host;

      (mid1, mid2, midHost, host) = InitializePreset();
    }

    [Test]
    public void PlayerToHostSendDataTest()
    {
      IMiddleware mid1, mid2, midHost;
      Host host;

      (mid1, mid2, midHost, host) = InitializePreset();


      int executions = 0;
      Action<int, string> inputHandler = (playerIndex, input) =>
      {
        if (playerIndex == 0)
          Assert.AreEqual("player1 message", input);
        else if (playerIndex == 1)
          Assert.AreEqual("player2 message", input);

        executions++;
      };

      host.AddInputHandler(inputHandler);
      mid1.AddInputHandler((a, b) => { });
      mid2.AddInputHandler((a, b) => { });

      mid1.SendData("player1 message");
      mid2.SendData("player2 message");

      Assert.AreEqual(executions, 2);
    }

    [Test]
    public void HostToPlayerSendDataTest()
    {
      IMiddleware mid1, mid2, midHost;
      Host host;

      (mid1, mid2, midHost, host) = InitializePreset();


      int executions = 0;
      Action<int, string> inputHandler = (_, input) =>
      {
        Assert.AreEqual("host's message", input);
        executions++;
      };

      mid1.AddInputHandler(inputHandler);
      mid2.AddInputHandler(inputHandler);

      midHost.SendData("host's message");

      Assert.AreEqual(executions, 2);
    }

    [Test]
    public void RejectPlayingAtNotSelfTurn()
    {
      IMiddleware mid1, mid2, midHost;
      Host host;

      (mid1, mid2, midHost, host) = InitializePreset();

      bool executed = false;
      mid2.AddInputHandler((i, s) =>
      {
        executed = true;
        Assert.AreEqual("That's not your turn now", s);
      });
      mid2.SendData("kek");

      Assert.IsTrue(executed);
    }

    [Test]
    public void HostInputValidationTest()
    {
      IMiddleware mid1, mid2, midHost;
      Host host;

      (mid1, mid2, midHost, host) = InitializePreset();

      bool responseGot = false;
      mid1.AddInputHandler((sender, input) =>
      {
        Assert.AreEqual(input, "Command name is not listed in commandsMap");
        responseGot = true;
      });

      Client client = new Client(mid1);
      client.SendCommand("that's a wrong command");


      Assert.IsTrue(responseGot);
    }

    [Test]
    public void ProcessAndExecuteCommand1()
    {
      IMiddleware mid1, mid2, midHost;
      Host host;

      (mid1, mid2, midHost, host) = InitializePreset();

      GameState state = new GameState(
        new Player[] {
          new Player(
            0,
            new List<Card> { },
            new List<Card> { },
            new List<Card> { Card.DimonCard(), Card.DimonCard(), Card.DimonCard() }
          ) ,
          new Player(
            1,
            new List<Card> { },
            new List<Card> { },
            new List<Card> { Card.DimonCard(), Card.DimonCard(), Card.DimonCard() }
          ) ,
        }
      );

      host.state = state;

      mid1.SendData("attack 1 2");


      GameState stateExpected = new GameState(
        new Player[] {
          new Player(
            0,
            new List<Card> { },
            new List<Card> { },
            new List<Card> { Card.DimonCard(), ((Func<Card>)(() => {
              var c = Card.DimonCard();
              c.HP -= c.Attack;
              c.IsSleeping = true;
              return c;
            }))() , Card.DimonCard() }
          ) ,
          new Player(
            1,
            new List<Card> { },
            new List<Card> { },
            new List<Card> { Card.DimonCard(), Card.DimonCard(), ((Func<Card>)(() => {
              var c = Card.DimonCard();
              c.HP -= c.Attack;
              return c;
            }))() }
          ) ,
        }
      );

      Assert.AreEqual(host.state, stateExpected);
    }

    public void ProcessAndExecuteCommand2()
    {
      IMiddleware mid1, mid2, midHost;
      Host host;

      (mid1, mid2, midHost, host) = InitializePreset();

      GameState state = new GameState(
        new Player[] {
          new Player(
            0,
            new List<Card> { },
            new List<Card> { },
            new List<Card> { Card.DimonCard(), Card.DimonCard(), Card.DimonCard() }
          ) ,
          new Player(
            1,
            new List<Card> { },
            new List<Card> { },
            new List<Card> { Card.DimonCard(), Card.DimonCard(), Card.DimonCard() }
          ) ,
        }
      );

      host.state = state;

      mid1.SendData("attack 1 2");


      GameState stateExpected = new GameState(
        new Player[] {
          new Player(
            0,
            new List<Card> { },
            new List<Card> { },
            new List<Card> { Card.DimonCard(), ((Func<Card>)(() => {
              var c = Card.DimonCard();
              c.HP -= c.Attack;
              return c;
            }))() , Card.DimonCard() }
          ) ,
          new Player(
            1,
            new List<Card> { },
            new List<Card> { },
            new List<Card> { Card.DimonCard(), Card.DimonCard(), ((Func<Card>)(() => {
              var c = Card.DimonCard();
              c.HP -= c.Attack;
              return c;
            }))() }
          ) ,
        }
      );

      Assert.AreEqual(host.state, stateExpected);
    }

    [Test]
    public void ProcessAndExecuteCommandEndTurn()
    {
      IMiddleware mid1, mid2, midHost;
      Host host;

      (mid1, mid2, midHost, host) = InitializePreset();

      GameState state = new GameState(
        new Player[] {
          new Player(
            0,
            new List<Card> { },
            new List<Card> { },
            new List<Card> { Card.DimonCard()}
          ) ,
          new Player(
            1,
            new List<Card> { },
            new List<Card> { },
            new List<Card> { }
          ) ,
        }
      );

      host.state = state;

      mid1.SendData("end");


      GameState stateExpected = new GameState(
        new Player[] {
          new Player(
            0,
            new List<Card> { },
            new List<Card> { },
            new List<Card> { ((Func<Card>)(() => {
              var c = Card.DimonCard();
              c.IsSleeping = false;
              return c;
            }))() }
          ) ,
          new Player(
            1,
            new List<Card> { },
            new List<Card> { },
            new List<Card> { }
          ) ,
        }
      );

      stateExpected.CurrentPlayer = stateExpected.Players[1];

      Assert.AreEqual(host.state, stateExpected);
    }


  }
}