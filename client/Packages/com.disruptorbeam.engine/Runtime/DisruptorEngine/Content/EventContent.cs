using System.Collections.Generic;
using Beamable.Common.Content;

namespace Beamable.Content
{
    [ContentType("events")]
    [System.Serializable]
    public class EventContent : ContentObject
    {
        public new string name;
        public string start_date;
        public int partition_size;
        public List<EventPhase> phases;
        public List<EventScoreReward> score_rewards;
        public List<EventRankReward> rank_rewards;
        public List<StoreRef> stores;
    }

    [System.Serializable]
    public class EventRef : ContentRef<EventContent>
    {

    }

    [System.Serializable]
    public class EventPhase
    {
        public string name;
        public int duration_minutes;
        public List<EventRule> rules;
    }

    [System.Serializable]
    public class EventRule
    {
        public string rule;
        public string value;
    }

    [System.Serializable]
    public class EventScoreReward
    {
        public int min;
        public List<EventObtain> obtain;
    }

    [System.Serializable]
    public class EventRankReward
    {
        public int min;
        public int max;
        public List<EventObtain> obtain;
    }

    [System.Serializable]
    public class EventObtain
    {
        public string symbol;
        public int count;
    }
}