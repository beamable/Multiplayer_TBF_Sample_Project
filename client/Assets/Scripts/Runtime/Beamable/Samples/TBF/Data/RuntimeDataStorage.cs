using Beamable.Samples.Core.Components;

namespace Beamable.Samples.TBF.Data
{
	/// <summary>
	/// Store game-related data which survives across scenes
	/// </summary>
	public class RuntimeDataStorage : SingletonMonobehavior<RuntimeDataStorage>
	{

		//  Properties  ----------------------------------
		public long LocalPlayerDbid { get { return _localPlayerDbid; } set { _localPlayerDbid = value; } }
		public string MatchId { get { return _matchId; } set { _matchId = value; } }
		public int TargetPlayerCount { get { return _targetPlayerCount; } set { _targetPlayerCount = value; } }
      public bool IsMatchmakingComplete { get { return _isMatchmakingComplete; } set { _isMatchmakingComplete = value; } }

		//  Fields  --------------------------------------
		public const int UnsetPlayerCount = -1;
		private bool _isMatchmakingComplete;
		private long _localPlayerDbid;
		private string _matchId;
		private int _targetPlayerCount;

		//  Unity Methods  --------------------------------

		protected override void Awake()
		{
			base.Awake();
			ClearData();
		}

		//  Other Methods  --------------------------------

		/// <summary>
		/// Demonstrates that the lifecycle of data is runtime only
		/// </summary>
		private void ClearData()
      {
			_isMatchmakingComplete = false;
			_localPlayerDbid = 0;
			_matchId = "";
			_targetPlayerCount = UnsetPlayerCount;
		}
   }
}
