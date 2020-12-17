using Beamable.Samples.TBF.Multiplayer.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Samples.TBF.Data
{
   /// <summary>
   /// Store data related to: Game progress
   /// 
   /// While TBF works with only 1 or 2 players, the data structures and algorithms
   /// work with a higher player count to illustrate how to scale up a game.
   /// </summary>
   [Serializable]
   public class GameProgressData
   {
      //  Properties  --------------------------------------
      public bool CurrentRoundHasWinnerPlayerDbid {  get { return CurrentRoundWinnerPlayerDbid != TBFConstants.UnsetValue;  } }
      public long CurrentRoundWinnerPlayerDbid {  get { return _currentRoundWinnerPlayerDbid;  } }


      public bool GameHasWinnerPlayerDbid
      {
         get
         {
            if (CurrentRoundNumber == _configuration.GameRoundsTotal)
            {
               return true;
            }
            else
            {
               return false;
            }
         }
      }

      public long GameWinnerPlayerDbid
      {
         get
         {
            int roundsWon01 = RoundsWonByPlayerDbid.Values.First();
            int roundsWon02 = RoundsWonByPlayerDbid.Values.Last();

            if (TBFConstants.IsDebugLogging)
            {
               Debug.Log($"GetGameWinnerPlayerDbid() Player1:{roundsWon01} Player2:{roundsWon02}.");
            }

            if (roundsWon01 > roundsWon02)
            {
               return RoundsWonByPlayerDbid.Keys.First(); //ex. Key = 2, Value = dbid2342342
            }
            else
            {
               return RoundsWonByPlayerDbid.Keys.Last();
            }
         }
      }


      //  Fields  --------------------------------------

      /// <summary>
      /// All game moves by DBID for the current ROUND.
      /// </summary>
      public Dictionary<long, GameMoveEvent> GameMoveEventsThisRoundByPlayerDbid = new Dictionary<long, GameMoveEvent>();

      /// <summary>
      /// The total number of ROUNDS won in the current GAME by DBID
      /// </summary>
      public Dictionary<long, int> RoundsWonByPlayerDbid = new Dictionary<long, int>();

      public int CurrentRoundNumber = 0;
      private long _currentRoundWinnerPlayerDbid = TBFConstants.UnsetValue;
      private Configuration _configuration;

      //  Constructor  ---------------------------------
      public GameProgressData (Configuration configuration)
      {
         _configuration = configuration;
      }

      //  Other Methods  -------------------------------
      public void EvaluateGameMoveEventsThisRound()
      {
         _currentRoundWinnerPlayerDbid = TBFConstants.UnsetValue;

         //One player game
         if (GameMoveEventsThisRoundByPlayerDbid.Count == 1)
         {
            _currentRoundWinnerPlayerDbid = GameMoveEventsThisRoundByPlayerDbid.First().Key;
         }
         //Two player game
         else if (GameMoveEventsThisRoundByPlayerDbid.Count == 2)
         {
            GameMoveEvent gameMoveEvent01 = GameMoveEventsThisRoundByPlayerDbid.Values.First();
            GameMoveEvent gameMoveEvent02 = GameMoveEventsThisRoundByPlayerDbid.Values.Last();

            if (gameMoveEvent01.GameMoveType == gameMoveEvent02.GameMoveType)
            {
               //Game is a tie, increment NOONE as winner
               return;
            }
            else
            {
               if (gameMoveEvent01.GameMoveType == GameMoveType.High ||
                  gameMoveEvent02.GameMoveType == GameMoveType.Medium)
               {
                  _currentRoundWinnerPlayerDbid = gameMoveEvent01.PlayerDbid;
               }
               else if (gameMoveEvent01.GameMoveType == GameMoveType.Medium ||
                        gameMoveEvent02.GameMoveType == GameMoveType.Low)
               {
                  _currentRoundWinnerPlayerDbid = gameMoveEvent01.PlayerDbid;
               }
               else if (gameMoveEvent01.GameMoveType == GameMoveType.Low ||
                        gameMoveEvent02.GameMoveType == GameMoveType.High)
               {
                  _currentRoundWinnerPlayerDbid = gameMoveEvent01.PlayerDbid;
               }
               else
               {
                  _currentRoundWinnerPlayerDbid = gameMoveEvent02.PlayerDbid;
               }
            }
         }
         else
         {
            throw new Exception("Bad game state");
         }

         if (!RoundsWonByPlayerDbid.ContainsKey(_currentRoundWinnerPlayerDbid))
         {
            RoundsWonByPlayerDbid.Add(_currentRoundWinnerPlayerDbid, 0);
         }
         RoundsWonByPlayerDbid[_currentRoundWinnerPlayerDbid]++;
      }

   }
}