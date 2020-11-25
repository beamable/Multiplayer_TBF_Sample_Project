using System.Collections.Generic;
using Beamable.Common;
using Beamable.Editor.Content.UI.ProjectView;
using Beamable.Editor.Environment;
using Beamable.Modules.Theme;
using Beamable.Platform.SDK;
using UnityEditor;

using UnityEngine;

#if UNITY_2018

using UnityEditor.Experimental.UIElements;

using UnityEngine.Experimental.UIElements;

using UnityEngine.Experimental.UIElements.StyleSheets;

#elif UNITY_2019_1_OR_NEWER

using UnityEditor.UIElements;

using UnityEngine.UIElements;

using UnityEngine.UIElements.StyleSheets;

#endif
using Object = UnityEngine.Object;



namespace Beamable.Editor.Toolbox

{

    public delegate void ToolboxAnimationRequest(IEnumerator<int> arg);



    public class ToolboxVisualElement : VisualElement

    {

        private const string Asset_UXML_ToolboxWidget =

            "Packages/com.disruptorbeam.engine/Editor/Toolbox/ToolboxWidget.uxml";



        private const string Asset_UXML_ToolboxCategory =

            "Packages/com.disruptorbeam.engine/Editor/Toolbox/toolboxCategory.uxml";



        private const string Asset_UXML_ToolboxWindow =

            "Packages/com.disruptorbeam.engine/Editor/Toolbox/ToolboxWindow.uxml";



        private const string Asset_USS_ToolboxWindow =

            "Packages/com.disruptorbeam.engine/Editor/Toolbox/Toolboxwindow.uss";

        private const string Asset_Configuration = "Packages/com.disruptorbeam.engine/Editor/Toolbox/defaultToolbox.asset";



        private const string USS_Highlight = "highlight";

        private const string USS_Light = "light";

        private const string USS_Dark = "dark";





        private ToolboxConfiguration _rawConfiguration;

        private ToolboxConfiguration Configuration

        {

            get

            {

                if (_rawConfiguration == null)

                {

                    _rawConfiguration = AssetDatabase.LoadAssetAtPath<ToolboxConfiguration>(Asset_Configuration);

                }



                return _rawConfiguration;

            }

        }



        private VisualTreeAsset _widgetTreeAsset, _toolboxWindowTree, _categoryTreeAsset;

        private Dictionary<VisualElement, float> _categoryHeights = new Dictionary<VisualElement, float>();



        public event ToolboxAnimationRequest OnAnimationRequested = (a) => { };



        public ToolboxVisualElement()

        {

            _widgetTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Asset_UXML_ToolboxWidget);

            _toolboxWindowTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Asset_UXML_ToolboxWindow);

            _categoryTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Asset_UXML_ToolboxCategory);

            Refresh();

        }



        private void Update()

        {

            // TODO THIS ONLY HERE TEMPORARILY, UNTIL WE FIGURE OUT A BETTER REFRESH MECHANIC

            if (Configuration.ForceUpdate)

            {

                Refresh();

                Configuration.ForceUpdate = false;

            }

        }



        public void Refresh()

        {

            // Each editor window contains a root VisualElement object

            VisualElement root = this;

            root.Clear();

            _categoryHeights.Clear();



            var mainNode = _toolboxWindowTree.CloneTree();

            name = nameof(ToolboxVisualElement);

            root.name = "main";

            root.AddStyleSheet(Asset_USS_ToolboxWindow);

            root.Add(mainNode);



            var style = mainNode.Q<InspectorElement>(name = "style");

            style.Bind(new SerializedObject(ThemeConfiguration.Instance));



            var listRoot = root.Query<ScrollView>(name = "category-listings").First(); // TODO this doesn't exist before the root has the main node



            var categories = FindCategories();

            for (var i = 0; i < categories.Count; i++)

            {

                var category = categories[i];

                var element = CreateCategoryElement(category, _categoryTreeAsset, i);

                listRoot.Add(element);

            }

            var portalButton = mainNode.Q<Button>("open-portal");
            portalButton.text = (ContentConstants.OPEN + " " + ContentConstants.PORTAL).ToUpper();
            portalButton.clickable.clicked += () => GetPortalUrl.Then(Application.OpenURL);
            var m = new ContextualMenuManipulator(rightClickEvt =>
            {
                rightClickEvt.menu.BeamableAppendAction("Copy Url",
                    mp => { GetPortalUrl.Then(url => { EditorGUIUtility.systemCopyBuffer = url; }); });
            }) {target = portalButton};

            var contentButton = mainNode.Q<Button>("open-content-list");
            contentButton.text = (ContentConstants.OPEN + " " + ContentConstants.CONTENT_MANAGER).ToUpper();
            contentButton.clickable.clicked += () => { ContentListWindow.Init(); };
        }

        private Promise<string> GetPortalUrl => EditorAPI.Instance.Map(de =>
            $"{BeamableEnvironment.PortalUrl}/{de.Cid}?refresh_token={de.Token.RefreshToken}");

        VisualElement CreateCategoryElement(ToolboxCategory category, VisualTreeAsset template, int index)

        {

            var node = template.CloneTree();

            node.Q<Label>(className: "category-name").text = category.Name;

            node.Q<Label>(className: "category-desc").text = category.Description;

            node.Q<Image>(className: "category-icon").SetImage(category.Icon);





            var header = node.Q<VisualElement>(className: "category-header");

            var listRoot = node.Q<VisualElement>(className: "category-list");

            header.RegisterCallback<MouseDownEvent, VisualElement>(ToggleCategory, listRoot); ;

            var widgets = FindWidgets(category);

            for (var i = 0; i < widgets.Count; i++)

            {

                var widget = widgets[i];

                var element = CreateWidgetElement(widget, _widgetTreeAsset, i);

                listRoot.Add(element);

            }



            listRoot.style.maxHeight = category.OpenByDefault ? int.MaxValue : 0;



            return node;

        }



        VisualElement CreateWidgetElement(ToolboxWidget widget, VisualTreeAsset template, int index)

        {

            var node = template.CloneTree(); // TODO we _could_ use slots, but I honestly don't see the point.

            node.Q<Label>(className: "widget-name").text = widget.Name;

            node.Q<Label>(className: "widget-desc").text = widget.Description;

            node.Q<Image>(className: "widget-icon").SetImage(widget.Icon);



            var contentNode = node.Q(className:"widget-content");

            contentNode.AddToClassList(index % 2 == 1 ? USS_Dark : USS_Light);



            node.userData = widget;

            node.RegisterCallback<MouseDownEvent, VisualElement>((evt, widgetNode) =>

            {

                contentNode.RemoveFromClassList(USS_Highlight);

                DragDFab(evt, widgetNode);

            }, node);

            node.RegisterCallback<MouseEnterEvent>(evt =>

            {

                contentNode.AddToClassList(USS_Highlight);

            });

            node.RegisterCallback<MouseLeaveEvent>(evt =>

            {

                contentNode.RemoveFromClassList(USS_Highlight);

            });



            return node;

        }



        void ToggleCategory(IMouseEvent evt, VisualElement categoryListNode)

        {

            var maxHeight = categoryListNode.style.GetMaxHeight();

            var flippedMax = int.MaxValue - maxHeight;

            var imageNode = categoryListNode.parent.Q<Image>(className: "drop-down");

            categoryListNode.style.maxHeight = flippedMax;

            if (flippedMax > 1) {
                imageNode.AddToClassList("open");
            }
            else {
                imageNode.RemoveFromClassList("open");
            }
        }



        IEnumerator<int> AnimateCategoryResize(VisualElement element, float startHeight, float targetHeight, float duration=.25f, float totalSteps=10)

        {

            yield return 0;



            var startTime = EditorApplication.timeSinceStartup;



            var timePerStep = duration / totalSteps;

            var currStep = 0;



            while (currStep <= totalSteps)

            {

                var nextTime = startTime + currStep * timePerStep;



                while (EditorApplication.timeSinceStartup < nextTime)

                {

                    // wait...

                    yield return currStep;

                }



                var completionRatio = currStep / totalSteps;



                var height = startHeight + completionRatio * (targetHeight - startHeight);

                element.style.maxHeight = height;

                currStep += 1;

            }

        }



        void DragDFab(IMouseEvent evt, VisualElement widgetNode)

        {

            var widget = widgetNode.userData as ToolboxWidget;

            DragAndDrop.PrepareStartDrag();

            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

            DragAndDrop.objectReferences = new Object[] {widget.DFab};

            DragAndDrop.StartDrag($"Create {widget.Name}");

        }



        List<ToolboxCategory> FindCategories()

        {

            return Configuration.Categories;

//            var set = new List<ToolboxCategory>();

//            for (var i = 0; i < 5; i++)

//            {

//                set.Add(Configuration.Categories[ i % Configuration.Categories.Count] );

//            }



//            return set;

        }



        List<ToolboxWidget> FindWidgets(ToolboxCategory category)

        {

            return category.Widgets;



//            var set = new List<ToolboxWidget>();

//            for (var i = 0; i < 3; i++)

//            {

//                set.Add(category.Widgets[ i % category.Widgets.Count ]);

//            }

//

//            return set;

        }



    }

}