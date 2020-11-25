using System.Collections;
using System.Collections.Generic;
using Beamable.Platform.SDK.Leaderboard;
using Beamable;
using TMPro;
using UnityEngine;

public class LeaderboardItem : MonoBehaviour
{
    public TextMeshProUGUI TxtAlias;
    public TextMeshProUGUI TxtRank;
    public TextMeshProUGUI TxtScore;

    public async void Apply(RankEntry entry)
    {
        var de = await API.Instance;
        var stats = await de.Stats.GetStats("client", "public", "player", entry.gt);
        string alias;
        stats.TryGetValue("alias", out alias);
        TxtAlias.text = alias;
        TxtRank.text = entry.rank.ToString();
        TxtScore.text = entry.score.ToString();
    }
}
