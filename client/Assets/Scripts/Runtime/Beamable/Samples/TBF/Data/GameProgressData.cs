﻿using Beamable.Samples.TBF.Exceptions;
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
      public enum RoundResult
      {
         Null,
         Winner,
         Tie
      }

      //  Properties  --------------------------------------
      public int CurrentRoundNumber { get { return _currentRoundNumber; } }
      public int CurrentRoundGameMoveEventCount { get { return _currentRoundGameMoveEventsByPlayerDbid.Count; } }
      public RoundResult CurrentRoundResult { get { return _currentRoundResult; } }
      public bool CurrentRoundHasWinnerPlayerDbid {  get { return CurrentRoundWinnerPlayerDbid != TBFConstants.UnsetValue;  } }
      public long CurrentRoundWinnerPlayerDbid {  get { return _currentRoundWinnerPlayerDbid;  } }


      public bool GameHasWinnerPlayerDbid
      {
         get
         {
            if (_currentRoundNumber == _configuration.GameRoundsTotal)
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
      private Dictionary<long, GameMoveEvent> _currentRoundGameMoveEventsByPlayerDbid = new Dictionary<long, GameMoveEvent>();

      /// <summary>
      /// The total number of ROUNDS won in the current GAME by DBID
      /// </summary>
      public Dictionary<long, int> RoundsWonByPlayerDbid = new Dictionary<long, int>();

      private int _currentRoundNumber = 0;
      private long _currentRoundWinnerPlayerDbid = TBFConstants.UnsetValue;
      private Configuration _configuration;
      private RoundResult _currentRoundResult = RoundResult.Null;

      //  Constructor  ---------------------------------
      public GameProgressData (Configuration configuration)
      {
         _configuration = configuration;
      }

      //  Other Methods  -------------------------------
      public void StartGame()
      {
         _currentRoundNumber = 0;
      }

      public void StartNextRound()
      {
         //Advance to the next round number, EXCEPT when
         //LAST round was a TIE
         if (_currentRoundResult != RoundResult.Tie)
         {
            _currentRoundNumber++;
         }

         _currentRoundResult = RoundResult.Null;
         _currentRoundWinnerPlayerDbid = TBFConstants.UnsetValue;
         _currentRoundGameMoveEventsByPlayerDbid.Clear();
      }

      public void AddCurrentRoundGameMoveEvent (GameMoveEvent gameMoveEvent)
      {
         if (_currentRoundGameMoveEventsByPlayerDbid.ContainsKey (gameMoveEvent.PlayerDbid))
         {
            throw new ArgumentException("AddGameMoveEventThisRound() This GameMoveEvent has already been added.");
         }
         _currentRoundGameMoveEventsByPlayerDbid[gameMoveEvent.PlayerDbid] = gameMoveEvent;
      }

      public GameMoveEvent GetCurrentRoundGameMoveEvent(long playerDbid)
      {
         GameMoveEvent remoteGameEvent;
         _currentRoundGameMoveEventsByPlayerDbid.TryGetValue(playerDbid, out remoteGameEvent);
         return remoteGameEvent;
      }

      public void EvaluateGameMoveEventsThisRound()
      {
         GameMoveEvent gameMoveEvent01 = _currentRoundGameMoveEventsByPlayerDbid.Values.First();
         GameMoveEvent gameMoveEvent02 = _currentRoundGameMoveEventsByPlayerDbid.Values.Last();

         if (TBFConstants.IsDebugLogging)
         {
            Debug.Log($"EvaluateGameMoveEventsThisRound() Player1 {gameMoveEvent01.GameMoveType}, Player2 {gameMoveEvent02.GameMoveType}.");
         }

         if (gameMoveEvent01.GameMoveType == gameMoveEvent02.GameMoveType)
         {
            //RESULT: Tie
            _currentRoundResult = RoundResult.Tie;
            return;
         }
         else
         {
            _currentRoundResult = RoundResult.Winner;

            //RESULT: Winner #1
            if (gameMoveEvent01.GameMoveType == GameMoveType.High &&
               gameMoveEvent02.GameMoveType == GameMoveType.Medium)
            {
               _currentRoundWinnerPlayerDbid = gameMoveEvent01.PlayerDbid;
            }
            //RESULT: Winner #1
            else if (gameMoveEvent01.GameMoveType == GameMoveType.Medium &&
                     gameMoveEvent02.GameMoveType == GameMoveType.Low)
            {
               _currentRoundWinnerPlayerDbid = gameMoveEvent01.PlayerDbid;
            }
            //RESULT: Winner #1
            else if (gameMoveEvent01.GameMoveType == GameMoveType.Low &&
                     gameMoveEvent02.GameMoveType == GameMoveType.High)
            {
               _currentRoundWinnerPlayerDbid = gameMoveEvent01.PlayerDbid;
            }
            //RESULT: Winner #2
            else
            {
               _currentRoundWinnerPlayerDbid = gameMoveEvent02.PlayerDbid;
            }
         }

         if (!RoundsWonByPlayerDbid.ContainsKey(_currentRoundWinnerPlayerDbid))
         {
            RoundsWonByPlayerDbid.Add(_currentRoundWinnerPlayerDbid, 0);
         }
         RoundsWonByPlayerDbid[_currentRoundWinnerPlayerDbid]++;
      }
   }
}