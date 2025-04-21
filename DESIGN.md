# dm4c-fivem - feature set design models

## misc mod ideas
    - turret defence (like starcraft UMS)
        - police constantly spawn from one end of a street
            constantly running after towards you
        - spawn count & weapons type increase gradually
        - need to try to fight them off as long as possible

    - tank vs choppers
        - choppers constantly spawn on venice beach
        - you have to use a tank to fight them off

    - plane dash
        - planes randomly spawn around you crashing to the ground
        - try to run from one end of a street to another w/o dying
        - OR maybe spwan from sky dive and race planes to the ground

    - sky dive maze
        - planes and misc object spwan randomly as you fall
        - try to make it safely to the ground

    - frogger with cars going 1000 MPH
        - set all car speeds to 1000 MPH
        - try to cross the street

    - death match 4 cash
        - every bullet is worth 1 penny
        - kill people and pickup their dropped ammo (pennies)
        - cashout with your winnings

## additional features
    - phone frame work to call your friends
    - 
## primary feature set design
    - intial home/starting area (single street with 2 exits)
        - no fighting or firing bullets in this area

        - 2 street exits to enter game modes (1 on each side)
            - 1) enter live mode (uses $BULLET tokens ammo)
                    - triggers DM4C validates existing mapping (EOA => cfx player ID)
            - 2) enter skirmish mode (uses free skirmish ammo)

        - has a single store-front, utilized for acquiring ammo
            - 1) skirmish ammo: request free 
                    - limited amount received each time (admin set)
                    - need to come back for more after being killed
            - 2) $BULLET ammo: CLAIM 'tebex.io' purchased $BULLET ammo (via cfx player ID)
                    - req in-game: register EOA (bound to current cfx player ID)
                        - triggers DM4C contract store mapping (EOA => cfx player ID)
                    - claim $BULLET ammo amount waiting via tebex purchase
                        - triggers DM4C contract 'transfer' of $BULLET tokens amount waiting to EOA 
            - 3) $BULLET ammo: via web3 dapp (ie. no in-game store-front or claim required)
                    - req web3-dapp: register cfx player ID (bound to invoking EOA)
                        - triggers DM4C contract store mapping (EOA => cfx player ID)
                    - user may purchase $BULLET tokens from web3 dapp 
                        - triggers DM4C contract 'transfer' $BULLET token ammo to EOA
                    - NOTE: any user may simply hold $BULLET tokens in their EOA 
                        - if EOA registered to cfx player ID
                            then, in-game $BULLET ammo is simply ready for use as 'reserve ammo'
                    
### game mode: live
    - acquiring $BULLET token ammo (via kills) should support instance Sonic chain cash-out
        - req in-game: register EOA (bound to current cfx player ID) w/ home store-front claim
            - OR - 
        - req web3-dapp: register cfx player ID (bound to invoking EOA)
        - in-game 'reserve ammo' = EOA wallet $BULLET token holdings (not yet fireable in-game)
        - in-game 'live ammo' = cfx player ID (bound to EOA) ammo that is indeed fireable in-game
        - in-game 'load ammo' = user manually selects to move ammo from 'reserve' to 'live'
            - triggers EOA wallet $BULLET tokens 'transfer' to DM4C contract escrow holding
            - NOTE: need 'ERC20 allowaance' work-around?
        - in-game 'kill' = user drops all 'live ammo' (NOTE: no 'reserve ammo' dropped)
        - in-game 'ammo pickup' = user acquires dropped 'live ammo', gets stored to them as 'reserve ammo'
            - triggers DM4C contract 'transfer' $BULLET tokens to EOA (bound to cfx player ID)

    - mapping in-game ammo types -> on-chain $BULLET token value (1 $BULLET token = 1 penny)
        - 1 handgun ammo = 1 $BULLET token  = $0.01
        - 1 AR ammo      = 2 $BULLET tokens = $0.02
        - 1 shotgun ammo = 4 $BULLET tokens = $0.04
        - 1 sniper ammo  = 5 $BULLET tokens = $0.05
        - 1 grenade ammo = 7 $BULLET tokens = $0.07
        - 1 RL ammo      = 10 $BULLET tokens = $0.10

## game mode: skirmish
    - TODO: need tokenomics design
    - acquiring skirmish ammo (via kills) should lead to 
        - rewards w/ $DM4C tokens earned?
    - playing for a certain amount time + certain amount of kills (of people players w/ "high" kill count)
        - rewards w/ $FRAG tokens earned?

## tebex.io: $BULLET ammo purchase metrics
    - tebex fee taken from each purchase?
    - cost: USD per $BULLET ammo?
        - need to account for tebex fee 
            and still equate to $BULLET token = 1 penny on dexes (web3 dapp)
        - cost / amount received?
    - how do we collect tebex purchase?
    - how do we use that amount to back $BULLET token stable on dexes
        (1 $BULLET = 1 penny)