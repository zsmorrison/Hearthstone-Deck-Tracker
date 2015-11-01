#region

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Overlay
{
	/// <summary>
	/// Interaction logic for AnimatedCard.xaml
	/// </summary>
	public partial class AnimatedCard
	{
		public AnimatedCard(Card card)
		{
			InitializeComponent();
			DataContext = card;
		}

		public Card Card
		{
			get { return (Card)DataContext; }
		}

		public async Task Spawn(bool fadeIn)
		{
			if(fadeIn)
				await RunStoryBoard("StoryboardSpawn");
			else
			{
				ViewboxBackground.Opacity = 1;
				StackPanelText.Opacity = 1;
				Height = MaxHeight;
			}
			Card.Update();
		}

		public async Task Despawn(bool highlight)
		{
			if(highlight)
				await RunStoryBoard("StoryboardDespawn");
			else
				await RunStoryBoard("StoryboardDespawnNoHighlight");
			Card.Update();
		}

		public async Task Update(bool highlight)
		{
			if(highlight)
				await RunStoryBoard("StoryboardUpdate");
			Card.Update();
		}

		private List<string> _runningStoryBoards = new List<string>();
		public async Task RunStoryBoard(string key)
		{
			if(_runningStoryBoards.Contains(key))
				return;
			_runningStoryBoards.Add(key);
			var sb = (Storyboard)FindResource(key);
			sb.Begin();
			await Task.Delay(sb.Duration.TimeSpan);
			_runningStoryBoards.Remove(key);
		}
	}
}