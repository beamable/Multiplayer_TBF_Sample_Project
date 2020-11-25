using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Beamable.Spew.Editor
{
   public class SpewWindow : EditorWindow
   {
      //[MenuItem("Window/Beamable Utilities/Spew Settings")]
      static void Init()
      {
         // Get existing open window or if none, make a new one:
         SpewWindow window = (SpewWindow)EditorWindow.GetWindow(typeof(SpewWindow), false, "Spew Settings");
         window.Show();
      }

      [NonSerialized] // This shouldn't be necessary as the list is private, but Unity seems to special case lists.
      private List<string> _allFlags;

      private Vector2 scroll;

      void OnGUI()
      {
         Logger.filter = EditorPrefs.GetString("SpewFilter", "");
         EditorGUILayout.Space();

         using (new EditorGUILayout.HorizontalScope(GUI.skin.box, GUILayout.ExpandWidth(true)))
         {
            EditorGUILayout.LabelField("Filter: ", GUILayout.Width(80));
            string filter = EditorGUILayout.TextField(Logger.filter);
            if (Logger.filter != filter)
            {
               EditorPrefs.SetString("SpewFilter", filter);
               Logger.filter = filter;
            }
         }

         EditorGUILayout.Space();

         ScrapeAllFlags();

         using (var scrollView = new EditorGUILayout.ScrollViewScope(scroll))
         {
            scroll = scrollView.scrollPosition;
            List<string> flagState = GetFlagStates();
            for (int i = 0; i < _allFlags.Count; ++i)
            {
               string flag = _allFlags[i];
               bool state = flagState.Contains(flag);

               using (new EditorGUILayout.HorizontalScope(GUI.skin.box, GUILayout.ExpandWidth(true)))
               {
                  bool newState = GUILayout.Toggle(state, flag);
                  if (state != newState)
                  {
                     if (newState)
                     {
                        flagState.Add(flag);
                     }
                     else
                     {
                        flagState.Remove(flag);
                     }

                     SetFlagStates(flagState);
                  }
               }
            }
         }
      }

      List<string> GetFlagStates()
      {
         string s = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
         string[] split = s.Split(';');
         return new List<string>(split);
      }

      void SetFlagStates(List<string> flags)
      {
         var sb = new StringBuilder();
         for (int i = 0; i < flags.Count; ++i)
         {
            sb.Append(flags[i]);
            sb.Append(";");
         }

         PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, sb.ToString());
      }

      private void ScrapeAllFlags()
      {
         if(_allFlags != null) return;

         var allFlagsSet = new HashSet<string>();

         // TEMP: Scrape the Logger methods for Conditional attributes
         Type type = typeof(Logger);
         ScrapeConditionals(type, allFlagsSet);

         var taggedClasses = AttributeUtil.GetClassesInAllAssemblies<SpewLoggerAttribute>();
         for (int i = 0; i < taggedClasses.Count; ++i)
         {
            ScrapeConditionals(taggedClasses[i], allFlagsSet);
         }

         _allFlags = new List<string>(allFlagsSet);
         _allFlags.Sort();
      }

      private void ScrapeConditionals(Type type, HashSet<string> flags)
      {
         MethodInfo[] infos = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
         for (int i = 0; i < infos.Length; ++i)
         {
            object[] atr = infos[i].GetCustomAttributes(typeof(ConditionalAttribute), false);
            for (int j = 0; j < atr.Length; ++j)
            {
               var ca = atr[j] as ConditionalAttribute;
               if (ca != null)
               {
                  flags.Add(ca.ConditionString);
               }
            }
         }
      }
   }

   // This belongs in Core.Utility.AttributeUtil, however, unfinished asmdef work made this difficult.
   public static class AttributeUtil
   {
      public static List<Type> GetClassesInAllAssemblies<TAttr>(bool inherit = true) where TAttr : Attribute
      {
         return GetClassesInAssemblies<TAttr>(AppDomain.CurrentDomain.GetAssemblies(), inherit);
      }

      public static List<Type> GetClassesInAssemblies<Tattr> (IList<Assembly> assemblies, bool inherit = true) where Tattr : Attribute
      {
         var result = new List<Type>();

         for(int a = 0; a < assemblies.Count; ++a)
         {
            var assembly = assemblies[a];
            var types = assembly.GetTypes();
            for (int i = 0; i < types.Length; ++i)
            {
               var t = types[i];
               if (t.IsPublic) // CONSIDER: Accept BindingFlags?
               {
                  var att = t.GetCustomAttributes(typeof(Tattr), inherit);
                  if (att.Length > 0)
                  {
                     // CONSIDER: Callback form to avoid List?
                     result.Add(t);
                  }
               }
            }
         }

         return result;
      }
   }
}
