﻿#region

using System;
using System.Linq;
using System.Xml.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;

#endregion

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal class XmlHelper
	{
		internal static void AddData(string xmlFile, GameMetaData gameMetaData, GameStats stats)
		{
			var xml = XDocument.Load(xmlFile);
			var hsReplay = xml.Elements().FirstOrDefault(x => x.Name == XmlElements.HsReplay);
			if(hsReplay == null)
				return;
			hsReplay.SetAttributeValue(XmlAttributes.Build, gameMetaData?.HearthstoneBuild);
			var game = hsReplay.Elements().FirstOrDefault(x => x.Name == XmlElements.Game);
			if(game != null)
			{
				AddGameAttributes(game, gameMetaData, stats);
				AddPlayerAttributes(game, gameMetaData, stats);
			}
			xml.Save(xmlFile);
		}

		private static void AddPlayerAttributes(XElement game, GameMetaData gameMetaData, GameStats stats)
		{
			var player = game.Elements().FirstOrDefault(x => x.Name == XmlElements.Player && x.Attributes().Any(a => a.Name == XmlAttributes.Name && a.Value == stats?.PlayerName));
			if(stats?.Rank > 0)
				player?.SetAttributeValue(XmlAttributes.Rank, stats.Rank);
			if(gameMetaData?.LegendRank > 0)
				player?.SetAttributeValue(XmlAttributes.LegendRank, gameMetaData.LegendRank);
			if(player != null && stats != null && stats.DeckId != Guid.Empty)
				AddDeckList(player, stats);
			if(stats?.OpponentRank > 0)
				game.Elements().FirstOrDefault(x => x.Name == XmlElements.Player && x.Attributes().Any(a => a.Name == XmlAttributes.Name && a.Value == stats.OpponentName))?
					.SetAttributeValue(XmlAttributes.Rank, stats.OpponentRank);
		}

		private static void AddGameAttributes(XElement game, GameMetaData gameMetaData, GameStats stats)
		{
			if(stats != null)
			{
				var mode = HearthDbConverter.GetGameType(stats.GameMode);
				if(mode != GameType.GT_UNKNOWN)
					game.SetAttributeValue(XmlAttributes.Type, (int)mode);
			}
			game.SetAttributeValue(XmlAttributes.Id, gameMetaData?.GameId);
			game.SetAttributeValue(XmlAttributes.ServerAddress, gameMetaData?.ServerAddress);
			game.SetAttributeValue(XmlAttributes.ClientId, gameMetaData?.ClientId);
			game.SetAttributeValue(XmlAttributes.SpectateKey, gameMetaData?.SpectateKey);
		}

		private static void AddDeckList(XElement player, GameStats stats)
		{
			var deck = DeckList.Instance.Decks.FirstOrDefault(x => x.DeckId == stats.DeckId)?.GetVersion(stats.PlayerDeckVersion);
			if(deck == null)
				return;
			var xmlDeck = new XElement(XmlElements.Deck);
			foreach(var card in deck.Cards)
			{
				var xmlCard = new XElement(XmlElements.Card);
				xmlCard.SetAttributeValue(XmlAttributes.Id, card.Id);
				if(card.Count > 1)
					xmlCard.SetAttributeValue(XmlAttributes.Count, card.Count);
				xmlDeck.Add(xmlCard);
			}
			player.Add(xmlDeck);
		}

		private static class XmlAttributes
		{
			public const string Build = "build";
			public const string Id = "id";
			public const string ServerAddress = "x-address";
			public const string ClientId = "x-clientid";
			public const string SpectateKey = "x-spectateKey";
			public const string Count = "count";
			public const string Type = "type";
			public const string Rank = "rank";
			public const string LegendRank = "legendRank";
			public const string Name = "name";
		}

		private static class XmlElements
		{
			public const string HsReplay = "HSReplay";
			public const string Game = "Game";
			public const string Player = "Player";
			public const string Deck = "Deck";
			public const string Card = "Card";
		}
	}
}