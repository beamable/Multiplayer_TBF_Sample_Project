using Beamable.Platform.SDK.Announcements;
using Beamable;
using Beamable.UI.Scripts;
using TMPro;
using UnityEngine;

public class AnnouncementSummary : MonoBehaviour
{
   private MenuManagementBehaviour MenuManager;
   private AnnouncementView Announcement;
   public TextMeshProUGUI TxtTitle;
   public TextMeshProUGUI TxtBody;

   public void Apply(MenuManagementBehaviour menu, AnnouncementView view)
   {
      MenuManager = menu;
      Announcement = view;
      TxtTitle.text = view.title;
      TxtBody.text = view.body;
   }
}