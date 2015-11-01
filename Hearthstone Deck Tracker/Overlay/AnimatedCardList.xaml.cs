#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Overlay
{
	/// <summary>
	/// Interaction logic for AnimatedCardList.xaml
	/// </summary>
	public partial class AnimatedCardList
	{
		private readonly ObservableCollection<AnimatedCard> _animatedCards = new ObservableCollection<AnimatedCard>();

		public AnimatedCardList()
		{
			InitializeComponent();
		}

		private double _cardHeight = 35;

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
						card.MaxHeight = maxHeight;
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
			}
			else if(existing.Card.Count > 0)
			{
				await existing.Update(true);
				existing.Card.Count = 0;
			}
		}
	}
}