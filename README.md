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

    - search for running server in servers.fivem.net (via fivem windows client)
    - install GTAV via steam on windows
    - join game via fivem client on windows

## feature set design
    - game mdoe: $BULLET token purchase
        - buy ammo via tebex
            - cost / amount received?
        - collect ammo after kills
        - withdraw ammo to sonic chain EOA
            - handgun | rifle ammo: 1 bullet = 1 $BULLET token
    - game mode: no bullet tokens
        - playing for a certain amount time + certain amount of kills (of people players w/ "high" kill count)
            - earns $FRAG tokens
    
## init project & build (local mac osx - unix base .net core)
    $ dotnet --version
    $ cd .../dm4c-fivem
    $ dotnet new -i CitizenFX.Templates
    $ mkdir MyResource
    $ cd MyResource/
    $ dotnet new cfx-resource
    $ dotnet build