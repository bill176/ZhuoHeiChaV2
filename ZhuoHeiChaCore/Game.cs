using System;
using System.Collections.Generic;
using System.Linq;

namespace ZhuoHeiChaCore
{
    public class Game : IGame
    {
        private readonly List<Card> _remainingCards = new List<Card>();
        private readonly Dictionary<int, List<Card>> _cardsInHandByPlayerId = new Dictionary<int, List<Card>>();
        private readonly List<int> _remainingPlayers = new List<int>();
        private int _currentPlayer;

        // Key: source player id
        // Value: a dictionary with
        //      Key: target player id
        //      Value: number of cards to send
        private readonly Dictionary<int, Dictionary<int, int>> _returnTributeDependencyGraph = new Dictionary<int, Dictionary<int, int>>();

        // Key: source player id
        // Value: a dictionary with
        //      Key: target player id
        //      Value: list of cards to send
        private readonly Dictionary<int, Dictionary<int, List<Card>>> _returnTributeCardsBuffer = new Dictionary<int, Dictionary<int, List<Card>>>();

        private readonly ICardFactory _cardFactory;
        private readonly ICardHelper _cardHelper;

        public Game(ICardFactory cardFactory, ICardHelper cardHelper)
        {
            _cardFactory = cardFactory;
            _cardHelper = cardHelper;
        }

        public Dictionary<int, IEnumerable<Card>> SendCards(int sourcePlayerId, int targetPlayerId, IEnumerable<Card> cards)
        {
            var cardList = cards.ToList();

            // check if both ids are valid
            if (!_returnTributeDependencyGraph.TryGetValue(sourcePlayerId, out var receivingPlayers)
                || !receivingPlayers.TryGetValue(targetPlayerId, out var numOfCardsToSend))
                throw new ArgumentException($"Player {sourcePlayerId} cannot send cards to player {targetPlayerId}!");

            // check if sourcePlayer has the cards
            if (!PlayerHasCards(sourcePlayerId, cardList))
                throw new ArgumentException($"Player {sourcePlayerId} doesn't have all of the following cards: " +
                    $"{_cardHelper.ConvertCardsToString(cardList)}");

            // check if number of cards to be sent is correct
            if (cardList.Count != numOfCardsToSend)
                throw new ArgumentException($"Player {sourcePlayerId} is trying to send {cardList.Count} cards to " +
                    $"{targetPlayerId}. Expecting {numOfCardsToSend} cards.");

            // store the returned cards to the buffer
            if (!_returnTributeCardsBuffer.TryGetValue(sourcePlayerId, out var tributeCardsByTargetPlayerId))
            {
                _returnTributeCardsBuffer[sourcePlayerId] = new Dictionary<int, List<Card>>
                {
                    { targetPlayerId, cardList }
                };
            }
            else
            {
                tributeCardsByTargetPlayerId[targetPlayerId] = cardList;
            }

            // remove the dependency to avoid duplication
            _returnTributeDependencyGraph[sourcePlayerId].Remove(targetPlayerId);
            if (_returnTributeDependencyGraph[sourcePlayerId].Count == 0)
                _returnTributeDependencyGraph.Remove(sourcePlayerId);

            // process the buffer if all tributes have been sent
            var playerCardsDictionary = new Dictionary<int, IEnumerable<Card>>();
            if (_returnTributeDependencyGraph.Count == 0)
                playerCardsDictionary = ProcessTributeBuffer();

            return playerCardsDictionary;
        }

        /// <summary>
        /// Check if all of cards belong to the player
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="cards"></param>
        private bool PlayerHasCards(int playerId, IEnumerable<Card> cards)
        {
            return cards.All(card => _cardsInHandByPlayerId[playerId].Contains(card));
        }

        /// <summary>
        /// Commits the tribute card changes previously stored in the buffer
        /// </summary>
        /// <returns>A dictionary containing the list of players whose cards changed as well 
        /// as their updated cards</returns>
        private Dictionary<int, IEnumerable<Card>> ProcessTributeBuffer()
        {
            var playersWithModifiedCards = new HashSet<int>();
            foreach (var srcPlayerId in _returnTributeCardsBuffer.Keys)
            {
                playersWithModifiedCards.Add(srcPlayerId);
                foreach (var tgtPlayerId in _returnTributeCardsBuffer[srcPlayerId].Keys)
                {
                    playersWithModifiedCards.Add(tgtPlayerId);
                    foreach (var card in _returnTributeCardsBuffer[srcPlayerId][tgtPlayerId])
                    {
                        _cardsInHandByPlayerId[srcPlayerId].Remove(card);
                        _cardsInHandByPlayerId[tgtPlayerId].Add(card);
                    }
                }
            }

            var playerCardsDictionary = new Dictionary<int, IEnumerable<Card>>();

            // TODO: only sort players with added cards
            foreach (var player in playersWithModifiedCards)
            {
                _cardsInHandByPlayerId[player].Sort(Card.Comparator);
                playerCardsDictionary[player] = _cardsInHandByPlayerId[player];
            }

            return playerCardsDictionary;
        }
    }

    public interface IGame
    {
        Dictionary<int, IEnumerable<Card>> SendCards(int sourcePlayerId, int targetPlayerId, IEnumerable<Card> cards);
    }
}
