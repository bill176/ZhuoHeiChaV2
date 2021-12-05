# Examining Options for ZHC Notification System

## What we want to achieve
- client should create/connect to a room
- client should send 'ready' to the server
- game should start once it receives 'ready' from all clients
- game should send cards to players once the game starts
- player should play hand, and broadcast that hand to all the other players

## Overall Order
- set tribute list (order)
- clear cards from last round
- initialize cards for this round
- tribute
- ace go public
- play-card-and-check-ended loop
- ask to play one more round?

### play-card-and-check-ended loop
- send message to frontend to enable play-card button for the player (stored in backend)
- frontend sends list of card ids (CardType) to backend, in player entity
- backend checks if the cards are valid (including jump/empty hand)
    - valid -> delete cards from player, and broadcast cards played
    - not valid -> return an error response
- check for time limit
    - time is up -> no hand
- check ended
    - if ended: stores order of completion into `TributeList`
    - else: continue the loop (increment `currentPlayer`)

## Options
- we could try the Azure Web Pub Sub (instead of plain SignalR)

## Problems we (I) have
- how do I use it with my server?
- client is fine, as we simply have to subscribe to the pub sub service normally
- server problem: how do I use it with serverless?


## Things to try out
- durable functions with the Class Library we have
- add Pub Sub to durable functions


# Backend Design

## HTTP Endpoints

### Room
base = `/room`

#### POST: `[base]/create/{capacity:int}`
- creates a room entity in the backend with the specified capacity
- returns a JSON object with field `roomId`

#### GET: `[base]/{roomId}`
- gets information about the room (capacity, roomId, currently connected players, current game id, etc.)  
- returns the room information as a JSON object

---

### Player
base = `/room/{roomId}/player`

For simplicity, a player is valid only within the scope of a room. That is, a player is no longer defined after leaving the room.

#### POST: `[base]/create`
- body is a JSON object containing the field `playerName`
- create a new player with `playerName`
- return a JSON object with field `playerId`

#### POST: `[base]/{playerId}/setready`
- add player to list of active/ready players in the room
- register client to the web pub sub service
- when all the connected players in the room are ready, the game automatically starts
- returns `WebPubSubConnection` object

#### `POST: [base]/playcards`
- body: contains the cards to be played
- on success: trigger the Web Pub Sub to send another play event for the next player, updates the cards in game entity
- on failure: return error status and keep the countdown timer

#### `POST: [base]/returntribute`
- body: contains the cards to be returned for tribute
- on success: trigger next tribute or game start event on pub sub, update cards in game entity
- on failure: return error status and keep countdown timer

## Entities

### Room

| name | type |
|---|---|
| roomId | string |
| capacity | int |
| connectedPlayers | list of int |

### Player
| name | type |
|---|---|
| playerId | int |
| remainingCards | list of int |
| previousHand | list of int |
| hasBlackAce | bool |

### Game

| name | type |
|---|---|
| gameId | string (same as roomId for now) |
| currentPlayerId | int |
| playerIdsByPlayingOrder | list of int (order of players playing cards) |
| playerIdsByFinishingOrder | list of int (order of players finishing their cards) |

## WebPubSub Service

### Play-card notification
- send play-card notifications to the player that needs to play next when:
    - a previous player just successfully played cards, and there is a next player
    - the game starts
- starts a countdown timer at the same time as the notification is sent

### observe-card notification
- send observe-card notification to all the non-playing players when:
    - a player has successfully played cards

### send-tribute notification
- send send-tribute notification when tribute starts
- include tribute information: to whom this tribute (return) is for and how many cards are needed