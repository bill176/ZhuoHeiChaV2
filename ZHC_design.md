# Game & Game Service
We want to implement the following two components:

### ZhuoHeiChaCore.Game
A class for managing a game session. Its responsibilities include:
- initialize game
    - shuffle card deck
    - send out cards
- preprocess game
    - process tribute
        - (optional) notify players of their cards to be given away for tribute
        - notify players to return tribute
        - wait for responses from the players
    - configure game
        - ask players about AceGoPublic
        - show who is the Ace
- start game
    - notify a player to play cards         
    - set a time limit(15s), after 15s means play empty hands. Also if all players pass, the there should not be a time limit for the first player.
    - verify player response
    - commit changes to the game
    - jump to end game if conditions met
    - repeat
- end game
    - store winning order
    - ask players about playing one more round

In addition, the game class should also provide a public lock object for thread synchronization. 

### ZhuoHeiChaBackend.GameService
A (singleton) class for managing all active game sessions. It is responsible for reacting to client requests as well as keeping access to a single game session object thread-safe. It should expose the following functionalities:
- create new game session
    - requires the SignalR connections for players in the session 
    - create players
    - tell whether we the game can start
- send tribute
    - requires the game session id, target player, source player, cards
- configure game
    - collects responses from player regarding game options
        - such as AceGoPublic, PlayOneMoreRound, etc.
- play cards
- handle exception: OnDisconnectedAsync

Note that this class is also responsible for sending notifications to players (signalr). That is, this class has dependency on the hub class that we will use for server-initiated communication.

# Controllers

### ZhuoHeiChaBackend.GameController
An ASP.Net controller class for processing incoming HTTP requests from clients and invoking the corresponding methods in business logic class (in this case `GameService`). There should be a wrapper method for every action supported in `GameService`.

# Hubs

### ZhuoHeiChaBackend.PlayerHub
A class that manages signalr connections to clients. Should define a list methods to be called from the backend to the clients.
-  AskReturnTributeBackend
-  AskAceGoPublicBackend
-  AskForPlayBackend
- CreateErrorMessage
- SendCurrentCardListBackend
-  ShowCurrentPlayerTurnBackend
-  NotifyOthers and update the card list
    -  PlayerListUpdateBackend
-  showAceIdPlayerListBackend
-  AskPlayOneMoreRoundBackend
-  BreakGameBackend
-  ResetState

