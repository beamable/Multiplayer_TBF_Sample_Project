using System;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.Server.Editor.DockerCommands;
using Beamable.Platform.SDK;
using Beamable.Editor;
using UnityEngine;

namespace Beamable.Server.Editor
{
   public enum MicroserviceState
   {
      IDLE, BUILDING, RUNNING, STOPPING
   }

   public enum MicroserviceCommand
   {
      BUILD, START, STOP, COMPLETE
   }

   class MicroserviceTransition
   {
      readonly MicroserviceState CurrentState;
      readonly MicroserviceCommand Command;

      public MicroserviceTransition(MicroserviceState currentState, MicroserviceCommand command)
      {
         CurrentState = currentState;
         Command = command;
      }

      public override int GetHashCode()
      {
         return 17 + 31 * CurrentState.GetHashCode() + 31 * Command.GetHashCode();
      }

      public override bool Equals(object obj)
      {
         var other = obj as MicroserviceTransition;
         return other != null && this.CurrentState == other.CurrentState && this.Command == other.Command;
      }
   }

   public class MicroserviceStateMachine
   {
      Dictionary<MicroserviceTransition, MicroserviceState> transitions;

      public MicroserviceDescriptor ServiceDescriptor { get; }
      public MicroserviceState CurrentState { get; private set; }

      private DockerCommand _process, _logProcess;

      public bool UseDebug { get; set; }

      public MicroserviceStateMachine(MicroserviceDescriptor descriptor, MicroserviceState initialState = MicroserviceState.IDLE)
      {
         ServiceDescriptor = descriptor;
         CurrentState = initialState;

         if (initialState == MicroserviceState.RUNNING)
         {
            CaptureLogs();
         }


         transitions = new Dictionary<MicroserviceTransition, MicroserviceState>
         {
            { new MicroserviceTransition(MicroserviceState.IDLE, MicroserviceCommand.BUILD), MicroserviceState.BUILDING },
            { new MicroserviceTransition(MicroserviceState.BUILDING, MicroserviceCommand.COMPLETE), MicroserviceState.IDLE },
            { new MicroserviceTransition(MicroserviceState.IDLE, MicroserviceCommand.COMPLETE), MicroserviceState.IDLE },
            { new MicroserviceTransition(MicroserviceState.IDLE, MicroserviceCommand.START), MicroserviceState.RUNNING },
            { new MicroserviceTransition(MicroserviceState.RUNNING, MicroserviceCommand.STOP), MicroserviceState.STOPPING },
            { new MicroserviceTransition(MicroserviceState.RUNNING, MicroserviceCommand.COMPLETE), MicroserviceState.IDLE },
            { new MicroserviceTransition(MicroserviceState.STOPPING, MicroserviceCommand.COMPLETE), MicroserviceState.IDLE },
         };
      }


      Promise<MicroserviceState> StartBuilding()
      {
         StartProcess(new BuildImageCommand(ServiceDescriptor));
         return Promise<MicroserviceState>.Successful(MicroserviceState.BUILDING);
      }

      public Promise<MicroserviceState> MoveNext(MicroserviceCommand command)
      {
         CurrentState = GetNext(command);

         switch (CurrentState)
         {
            case MicroserviceState.BUILDING:
               StartProcess(new BuildImageCommand(ServiceDescriptor));
               break;
            case MicroserviceState.RUNNING:
               return EditorAPI.Instance.FlatMap(de => de.GetRealmSecret()).Map(secret =>
               {
                  var logLevel = UseDebug ? "Debug" : "Information";
                  StartProcess(new RunImageCommand(ServiceDescriptor, secret, logLevel));

                  // also, always start a log process...
                  // CaptureLogs();

                  return CurrentState;
               });
            case MicroserviceState.STOPPING:
               StartProcess(new StopImageCommand(ServiceDescriptor));
               break;
         }
         return Promise<MicroserviceState>.Successful(CurrentState);
      }

      MicroserviceState GetNext(MicroserviceCommand command)
      {
         var transition = new MicroserviceTransition(CurrentState, command);

         if (transitions.TryGetValue(transition, out var nextState))
         {
            return nextState;
         }

         throw new Exception("Invalid transition: " + CurrentState + " -> " + command);
      }


      void StartProcess(DockerCommand command)
      {
         _process?.Kill();
         _process = command;
         _process.OnExit += status => MoveNext(MicroserviceCommand.COMPLETE);
         _process.Start();
      }

      void CaptureLogs()
      {
         _logProcess?.Kill();
         _logProcess = new FollowLogCommand(ServiceDescriptor);
         _logProcess.OnExit += i => { };
         _logProcess.Start();
      }
   }
}