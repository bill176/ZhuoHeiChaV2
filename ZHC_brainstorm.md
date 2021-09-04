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