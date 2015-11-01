#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Overlay
{
	/// <summary>
	/// Interaction logic for AnimatedCardList.xaml
	/// </summary>
	public partial class AnimatedCardList : INotifyPropertyChanged
	{
		private readonly ObservableCollection<AnimatedCard> _animatedCards = new ObservableCollection<AnimatedCard>();

		public AnimatedCardList()
		{
			InitializeComponent();
		}

		private double _cardHeight = 35;

		public double MaxItemHeight
		{
			get { return Math.Min(35, MaxHeight / _animatedCards.Count); }
		}

		public void Update(List<Card> cards, bool player, bool reset)
		{
			if(reset)
			{
				_animatedCards.Clear();
				ItemsControl.Items.Clear();
			}
			foreach(var card in cards)
			{
				var existing = _animatedCards.FirstOrDefault(x => x.Card.EqualsForList(card));
				if(existing == null)
				{
					var newCard = new AnimatedCard(card);
					_animatedCards.Insert(cards.IndexOf(card), newCard);
					ItemsControl.Items.Insert(cards.IndexOf(card), newCard);
					OnPropertyChanged("MaxItemHeight");
					newCard.Spawn(!reset);
				}
				else if(existing.Card.Count != card.Count || existing.Card.HighlightInHand != card.HighlightInHand)
				{
					var highlight = existing.Card.Count != card.Count;
					existing.Card.Count = card.Count;
					existing.Card.HighlightInHand = card.HighlightInHand;
					existing.Update(highlight);
				}
			}
			foreach(var card in _animatedCards.Select(x => x.Card).ToList())
			{
				if(!cards.Any(x => x.EqualsForList(card)))
					RemoveCard(card, player);
			}
			Height = MaxItemHeight < 35 ? MaxHeight : double.NaN;
			UpdateSize();
		}

		public void UpdateSize()
		{
			if(_animatedCards.Count > 0 && !double.IsNaN(MaxHeight))
			{
				var maxHeight = Math.Min(35, MaxHeight / _animatedCards.Count);
				if(_cardHeight != maxHeight)
				{
					_cardHeight = maxHeight;
					foreach(var card in _animatedCards)
						card.UpdateMaxHeight(maxHeight);
				}
			}
		}

		private async void RemoveCard(Card card, bool player)
		{
			var existing = _animatedCards.FirstOrDefault(x => x.Card.EqualsForList(card));
			if(existing == null)
				return;
			if(Config.Instance.RemoveCardsFromDeck || !player)
			{
				await existing.Despawn(existing.Card.Count > 0);
				_animatedCards.Remove(existing);
				ItemsControl.Items.Remove(existing);
				OnPropertyChanged("MaxItemHeight");
			}
			else if(existing.Card.Count > 0)
			{
				await existing.Update(true);
				existing.Card.Count = 0;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}