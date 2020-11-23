using Beamable.Samples.TBF.Core;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Beamable.Samples.TBF.Audio
{
	/// <summary>
	/// Maintain a list of AudioSources and play the next 
	/// AudioClip on the first available AudioSource.
	/// </summary>
	public class SoundManager : SingletonMonobehavior<SoundManager>
	{
		[SerializeField]
		private List<AudioClip> _audioClips = new List<AudioClip>();

		[SerializeField]
		private List<AudioSource> _audioSources = new List<AudioSource>();

		protected override void Awake()
		{
			base.Awake();
			/// If/after updating AudioClips in the UnityEditor, run this once to rebuild const *.cs
			//DebugLogCodeSnippet();
		}

		/// <summary>
		/// Create a list to help in creating a constants class. Optional.
		/// </summary>
		private void DebugLogCodeSnippet()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("DebugLogCodeSnippet...");

			foreach (AudioClip audioClip in _audioClips)
         {
				stringBuilder.AppendLine($"public const string {audioClip.name} = \"{audioClip.name}\";");
         }

			Debug.Log(stringBuilder.ToString());
		}

		public void PlayAudioClip(string audioClipName, float pitch)
		{
			foreach (AudioClip audioClip in _audioClips)
			{
				if (audioClip.name == audioClipName)
				{
					PlayAudioClip(audioClip, pitch);
					return;
				}
			}
		}
		/// <summary>
		/// Play the AudioClip by name.
		/// </summary>
		public void PlayAudioClip(string audioClipName)
		{
			PlayAudioClip(audioClipName, 1);
		}

		/// <summary>
		/// Play the AudioClip by reference.
		/// If all sources are occupied, nothing will play.
		/// </summary>
		public void PlayAudioClip(AudioClip audioClip, float pitch)
		{
			foreach (AudioSource audioSource in _audioSources)
			{
				if (!audioSource.isPlaying)
				{
					audioSource.clip = audioClip;
					audioSource.pitch = pitch;
					audioSource.Play();
					return;
				}
			}
		}

		/// <summary>
		/// Play the AudioClip by reference.
		/// If all sources are occupied, nothing will play.
		/// </summary>
		public void PlayAudioClip(AudioClip audioClip)
		{
			PlayAudioClip (audioClip, 1);
		}
	}
}