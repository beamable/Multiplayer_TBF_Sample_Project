using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Editor.Content.Models;
using Beamable.Platform.SDK;
using Beamable.Content;
using Beamable.Content.Validation;
using Beamable.Editor;
using Beamable.Editor.Content;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Debug = Beamable.Common.Debug;
using UnityEditor;

namespace Beamable.Editor.Content
{
   public class ContentManager
   {
      public ContentDataModel Model { get; private set; }

      public void Initialize()
      {
         EditorAPI.Instance.Then(de =>
         {
            Model = new ContentDataModel(de.ContentIO);

            var localManifest = de.ContentIO.BuildLocalManifest();
            Model.SetLocalContent(localManifest);
            de.ContentIO.OnManifest.Then(manifest =>
            {
               Model.SetServerContent(manifest);

            });

            Model.SetContentTypes(ContentRegistry.GetAll().ToList());

            ValidateContent(null, null); // start a validation in the background.

            ContentIO.OnContentCreated += ContentIO_OnContentCreated;
            ContentIO.OnContentDeleted += ContentIO_OnContentDeleted;
            ContentIO.OnContentRenamed += ContentIO_OnContentRenamed;
         });
      }

      public void RefreshServer()
      {
         EditorAPI.Instance.Then(de =>
         {
            de.ContentIO.FetchManifest().Then(manifest =>
            {
               Model.SetServerContent(manifest);
            });
         });
      }

      public IContentObject AddItem()
      {
         Debug.Log("AddItem");
         TreeViewItem selectedTreeViewItem = Model.SelectedContentTypes.FirstOrDefault();
         ContentTypeTreeViewItem selectedContentTypeTreeViewItem = (ContentTypeTreeViewItem)selectedTreeViewItem;

         if (selectedContentTypeTreeViewItem == null)
         {
            Debug.LogError(new Exception("AddItem() failed. selectedContentTypeTreeViewItem must not be null."));
            return null;
         }

         return AddItem(selectedContentTypeTreeViewItem.TypeDescriptor);
      }

      public IContentObject AddItem(ContentTypeDescriptor typeDescriptor)
      {
         var itemType = typeDescriptor.ContentType;
         var itemName = ContentManagerContants.GetNameForNewContentFileByType(itemType);
         ContentObject content = ScriptableObject.CreateInstance(itemType) as ContentObject;
         content.SetContentName(itemName);

         Model.CreateItem(content);
         return content;
      }

      public Promise<List<ContentExceptionCollection>> ValidateContent(HandleContentProgress progressHandler, HandleValidationErrors errorHandler)
      {
         return EditorAPI.Instance.FlatMap(de =>
         {
            var contentValidator = new ContentValidator(de.ContentIO);
            var ctx = de.ContentIO.GetValidationContext();
            ContentObject.ValidationContext = ctx;
            var promise = contentValidator.Validate(ctx, Model.TotalContentCount, Model.GetAllContents(), progressHandler, errorHandler);
            promise.Then(_ => { Debug.Log("Validation complete"); });
            return promise;
         });
      }

      public Promise<Unit> PublishContent(ContentPublishSet publishSet, HandleContentProgress progressHandler, HandleDownloadFinished finishedHandler)
      {
         return EditorAPI.Instance.FlatMap(de =>
         {
            var promise = de.ContentPublisher.Publish(publishSet, progress =>
            {
               progressHandler?.Invoke(progress.Progress);
            });

            finishedHandler?.Invoke(promise);
            return promise.Map(_ =>
            {
               de.ContentIO.FetchManifest();
               return _;
            });
         });
      }


      public Promise<Unit> DownloadContent(DownloadSummary summary, HandleContentProgress progressHandler, HandleDownloadFinished finishedHandler)
      {
         //Resumes UI updates and other import behaviors
         void StopAssetEditing()
         {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
         }

         return EditorAPI.Instance.FlatMap(de =>
         {
            var contentDownloader = new ContentDownloader(de.Requester, de.ContentIO);
            //Disallow updating anything while importing / refreshing
            AssetDatabase.StartAssetEditing();
            var downloadPromise = contentDownloader.Download(summary, progressHandler).Then(_ =>
               StopAssetEditing()
            ).Error(_ =>
               StopAssetEditing()
            );
            finishedHandler?.Invoke(downloadPromise);
            return downloadPromise;
         });
      }

      /// <summary>
      /// Refresh the data and thus rendering of the <see cref="ContentManagerWindow"/>
      /// </summary>
      /// <param name="isHardRefresh">TODO: My though there is that false means keep the currently selected item. TBD if possible. - srivello</param>
      public void RefreshWindow(bool isHardRefresh)
      {
         Debug.Log($"RefreshWindow ({isHardRefresh})");

         if (isHardRefresh)
         {
            ContentManagerWindow.Init();
         }
         else
         {
            RefreshServer();
         }
      }

      public void ShowDocs()
      {
         Debug.Log("ShowDocs");
         Application.OpenURL(ContentConstants.URL_TOOL_WINDOW_CONTENT_MANAGER);
      }

      private void ContentIO_OnContentDeleted(IContentObject content)
      {
         Model.HandleContentDeleted(content);
      }

      private void ContentIO_OnContentCreated(IContentObject content)
      {
         Model.HandleContentAdded(content);
      }

      private void ContentIO_OnContentRenamed(string oldId, IContentObject content, string nextAssetPath)
      {
         Model.HandleContentRenamed(oldId, content, nextAssetPath);
      }

      public Promise<DownloadSummary> PrepareDownloadSummary(params ContentItemDescriptor[] filter)
      {
         // no matter what, we always want a fresh manifest locally and from the server.
         return EditorAPI.Instance.FlatMap(de =>
         {
            return de.ContentIO.FetchManifest().Map(serverManifest =>
            {
               var localManifest = de.ContentIO.BuildLocalManifest();



               return new DownloadSummary(de.ContentIO, localManifest, serverManifest, filter.Select(x => x.Id).ToArray());
            });
         });

      }

      public void Destroy()
      {
         ContentIO.OnContentCreated -= ContentIO_OnContentCreated;
         ContentIO.OnContentDeleted -= ContentIO_OnContentDeleted;
         ContentIO.OnContentRenamed -= ContentIO_OnContentRenamed;
      }

      public Promise<ContentPublishSet> CreatePublishSet()
      {
         return EditorAPI.Instance.FlatMap(de => de.ContentPublisher.CreatePublishSet());
      }
   }
}