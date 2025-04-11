# dm4c-fivem
dm4c c# fivem client/server integration



## test servers
    
## NEXT
    - start c# and lua scripting for DM4C requirements (needs finalized design)
    - TG: @the_xyt (github: breardon2011)
        I think the first step…
        Need a generic HUD using native cfx NUI framework to track reserve ammo vs. live/active ammo

        Reserve ammo will represent $BULLET tokens available for use in the users EOA wallet on-chain.

        Live ammo would represent $BULLETS that are active to be fired (ie. transferred on chain from user EOA to our smart contract) and would as well be dropped when the user is killed and able to be picked up by anyone else in the game, and added to “their” reserve ammo (ie. Transferred from our smart contract to their EOA wallet)

        The on-chain transfer parts would just be simulated to start… ie. No actual calls to smart contracts, but rather simple in-game chat commands for initially testing 

        ie. /reserve_to_live <amount>

        or something to that affect.

        And as well another command for going the other way (simulating someone cashing out on-chain)

        ie. /live_to_reserve <amount>

        Does that kinda make sense so far? 🤔

	- ready for continued design of feature sets 
		w/ c# updates and more lua scripting (if needed)

## feature set design
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
                    
    - game mode: live
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
        - mapping in-game ammo types -> on-chain $BULLET token value
            - 1 handgun ammo = 1 $BULLET token
            - 1 AR ammo      = 2 $BULLET tokens
            - 1 shotgun ammo = 3 $BULLET tokens
            - 1 grenade ammo = 4 $BULLET tokens
            - 1 RL ammo      = 5 $BULLET tokens

    - game mode: skirmish
        - TODO: need tokenomics design
        - acquiring skirmish ammo (via kills) should lead to 
            - rewards w/ $DM4C tokens earned?
        - playing for a certain amount time + certain amount of kills (of people players w/ "high" kill count)
            - rewards w/ $FRAG tokens earned?

    - tebex.io: $BULLET ammo purchase metrics
        - tebex fee taken from each purchase?
        - cost: USD per $BULLET ammo?
            - need to account for tebex fee 
                and still equate to $BULLET token = 1 penny on dexes (web3 dapp)
            - cost / amount received?
        - how do we collect tebex purchase?
        - how do we use that amount to back $BULLET token stable on dexes
            (1 $BULLET = 1 penny)

## build server-data (c# based) and deploy on remote linux server (ubuntu)
    - install .net core on remote server (ref: https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#set-environment-variables-system-wide)
        # NOTE_SUCCESS: appears to work fine on ubuntu 22.0.x but NOT ubuntu 24.0.x (but maybe i messed up and tried 'apt' first like grok suggested below)
            $ sudo su
            $ cd /srv
            $ wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
            $ chmod 744 dotnet-install.sh 
            $ ./dotnet-install.sh --version latest
            $ snap install dotnet-sdk
            $ snap install dotnet-sdk --classic
            $ dotnet --version
            $ dotnet --list-sdks
            $ dotnet --list-runtimes
            $ cd .../MyResources
            $ dotnet build

        - alt install .net core on remote server using 'apt' (ref: grok thread)
            # NOTE_FAILED: does not appear to be successful (tried on ubuntu 24.0.x)
                $ sudo apt update
                $ sudo apt install dotnet-runtime-8.0

    - clone git fivem project on remove server
        $ cd /srv
        $ git clone https://github.com/housing37/dm4c-fivem.git
    
    - build on remote linux server to generate .dll files (c# based)
        $ cd .../server-data/resources/MyResources
        $ dotnet build

    - install fivem on remote server & run (via run.sh)
        (ref: https://docs.fivem.net/docs/server-manual/setting-up-a-server-vanilla/#linux)
        $ cd .../server-data
        $ mkdir ../server
        $ cd ../server
        $ wget https://runtime.fivem.net/artifacts/fivem/build_proot_linux/master/14033-60505548e21b6d6e0844e02e571513e15bff5ccc/fx.tar.xz
        $ tar xf fx.tar.xz

    - deploy fivem server (via: run.sh +exec server.cfg)
        $ cd .../server-data
        $ ../server/run.sh +exec server.cfg
            note: use vanilla server.cfg (placed in .../server-data)
                https://docs.fivem.net/docs/server-manual/setting-up-a-server-vanilla/#linux)
            note: add vanilla lua resources from:
                https://github.com/citizenfx/cfx-server-data.git

    - install GTAV via steam on windows
    - search for running server in servers.fivem.net (via fivem windows client)
    - join game via fivem client on windows
    
## init project & build (local mac osx - unix base .net core)
    $ dotnet --version
    $ cd .../dm4c-fivem
    $ dotnet new -i CitizenFX.Templates
    $ mkdir MyResource
    $ cd MyResource/
    $ dotnet new cfx-resource
    $ dotnet build