# dm4c-fivem - ubuntu server setup (w/ .net core c# support)

## setup local VM (virtual box) ubuntu instance
    - install virtual box & ubuntu-22 live server instance
        - NOTE: check 'skip unattended install' (ie. do not use unattended install)
        - NOTE: enable openSSH default installation option (during ubuntu live server intallation)
                - this allows you to use the 'user-name' w/ the ssh login
                    NOTE: user-name & password will be created during ubuntu server install process
                - this also creates ssh configs & keys (maybe needed) in '/etc/ssh/'
    - run ubuntu instance

    - get VM ubuntu IP address (on ubuntu side)
        $ ifconfig
        NOTE: if virtual box network config is set to NAT
            > output will be 10.0.2.x (default to NAT)
        NOTE: if virtual box network config is set to Bridge Adapter
            > output will be 192.168.x.x (default to bridge, uses host mac OSx ip range)

    - Enable/test ssh connection (x2) -> Configure Port Forwarding / networking in VirtualBox
        Open VirtualBox, select your VM, and go to Settings > Network > Adapter 1.
            1) Set to NAT.
                Click Advanced > Port Forwarding.
                Add a rule:
                    Protocol: TCP
                    Host IP: 127.0.0.1 (localhost)
                    Host Port: 2222 (or any free port)
                    Guest IP: 10.0.2.x
                    Guest Port: 22
                Restart ubuntu instance (maybe; NOTE: no restart may seem to work but cause network lag issues)
                Get VM ubuntu IP address (on ubuntu side)
                    $ ifconfig
                    > output will be 10.0.2.x (default to NAT)
                SSH test from mac OSx parent to ubuntu VM child
                    $ ssh -p 2222 <user-name>@localhost
                NOTE: if ssh hangs, check if ssh server is running on ubuntu (start if needed)
                    $ sudo systemctl status ssh
                    $ sudo systemctl start ssh
                NOTE: check if ssh port 22 is open
                    $ sudo ss -tuln | grep 22
                NOTE: allow ssh in ubunut VM
                    $ sudo ufw allow 22

            2) Set to Bridged Adapter
                Simply select your Mac’s active interface (e.g., Wi-Fi or eth0 or en0 or enp0s3, etc.).
                Restart ubuntu instance (maybe; NOTE: no restart may seem to work but cause network lag issues)
                Get VM ubuntu IP address (on ubuntu side)
                    $ ifconfig
                    > output will be 192.168.x.x (default to bridge, uses host mac OSx ip range)
                NOTE: if no ip address is given, restart networking
                    $ sudo dhclient enp0s3
                SSH test from mac OSx parent to ubuntu VM child
                    $ ssh <user-name>@192.168.x.x
                    NOTE: password requested (set during ubuntu install)
                NOTE: if ssh hangs, check if ssh server is running on ubuntu (start if needed)
                    $ sudo systemctl status ssh
                    $ sudo systemctl start ssh
                NOTE: check if ssh port 22 is open
                    $ sudo ss -tuln | grep 22
                NOTE: allow ssh in ubunut VM
                    $ sudo ufw allow 22
                NOTE: if permission denied for user-name & password...
                    then, trying using ssh keys: found on ubuntu side in '/etc/ssh/'
                OPTIONAL: set static VM ubuntu IP (avoid VM’s IP changing e.g., DHCP reassigning)
                    STEP 1: open editor of config.yaml file
                    $ sudo emacs /etc/netplan/00-installer-config.yaml
                    
                    STEP 2: edit to something like this... (Use unused IP (e.g., 192.168.4.100) outside router’s DHCP range)
                        network:
                        ethernets:
                            enp0s3:
                            addresses:
                                - 192.168.4.100/24
                            gateway4: 192.168.4.1
                            nameservers:
                                addresses: [8.8.8.8, 8.8.4.4]
                        version: 2

                    STEP 3: apply changes
                        $ sudo netplan apply
    
    - Open VM ubunut-22 fivem port so windows fivem clients can connect to it
        $ sudo ufw allow 30120
        $ sudo ufw status
        $ sudo ss -tuln | grep 30120
        NOTE: test connect in-game (hit F8 to open CLI)
            > connect 192.168.4.x:30120 

    - mount fivem project directory to ubuntu side
        Set Up a Shared Folder on Your Mac:
            On your Mac, keep your dm4c-fivem/ repo folder where you edit it (e.g., .../git/dm4c-fivem/).
            Open VirtualBox, select your Ubuntu VM, and go to Settings > Shared Folders
            Click the “+” icon:
                Folder Path: Browse to your dm4c-fivem/ folder on your Mac (e.g., .../git/dm4c-fivem/MyResource).
                Folder Name: Call it something like 'dm4c-fivem'
                Ensure 'Auto-mount' is NOT checked (and use '/etc/fstab' ref below)
                    - OR - 
                Check 'Auto-mount' and 'Make Permanent' (and use 'mount -t vboxsf ...' cmd below)
                  -> NOTE_042325: this doesn't seem to be working correctly
                        and only allow temp mounts that are wipped after server restarts
                Mount Point: Leave blank or set to /mnt/dm4c-fivem (you’ll use this later)
            Save and start the VM.
        Mount the Shared Folder in Ubuntu (temp; every server restart requires $ sudo mount -t ...)
            ## NOTE: using perminant mount w/ Ensure 'Auto-mount' is NOT checked (above)
            $ sudo mkdir -p /mnt/dm4c-fivem
            $ emacs /etc/fstab
                > add line: dm4c-fivem /mnt/dm4c-fivem vboxsf defaults,uid=1000,gid=1000 0 0
                > add line: Desktop /mnt/dm4c-fivem vboxsf defaults,uid=1000,gid=1000 0 0
                NOTE: uid|gid=1000, 1000 is by default the first ubuntu user & root user = 0
                    (this doesn't seem to matter & initial testing is fine, but can check w/ '$ id')
            $ sudo mount -a
            $ cd /mnt/dm4c-fivem
            $ ls -la

                - OR -

            ## NOTE: using temp mount w/ Check 'Auto-mount' and 'Make Permanent' (above)
            $ sudo apt update
            $ sudo apt install -y virtualbox-guest-utils
            $ sudo mount -t vboxsf dm4c-fivem /mnt/dm4c-fivem
            $ cd /mnt/dm4c-fivem
            $ ls -la

        Link to mounted FiveM Resources to standard server deploy path
            $ ln -s /mnt/dm4c-fivem /srv/dm4c-fivem

    - Install .net core on ubuntu VM
        # ref: https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#set-environment-variables-system-wide
        # NOTE_SUCCESS: appears to work fine on ubuntu 18.0.x & 22.0.x, but NOT ubuntu 24.0.x (but maybe i messed up and tried 'apt' first like grok suggested below)
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
            NOTE: if the dotnet installs above hang or have issues, debug w/ ...
                ## check VM internet access
                    $ ping 8.8.8.8
                ## check microsoft internet access
                    $ ping builds.dotnet.microsoft.com
     - Test build & run fivem local ubuntu VM server
        $ cd /srv/dm4c-fivem/resources/MyResource
        $ dotnet build
        $ cd /srv/dm4c-fivem/server-data
        $ ../_server/run.sh +exec local.cfg

## setup local VM (virtual box) ubuntu instance -> yarn error with local ubuntu vm
    - NOTE_ERROR_041325 (yarn): yarn issue during local ubunut run (leaks to 'webpack' & 'chat' resource issue)
        - yarn seems to fail on local VM ubuntu-22, but NOT on remove AWS ubuntu-22 server
        - grok says this is a node.js requirement for loading and install dependencies
            and its likely an error due to virtual box lock files and security, etc.
                (ref: grok thread -> "_GTA fivem setup")
        - error seems to stem around using .../server-data/cache folder in a VM environment 
            note: issue doesn't seem to be isolated to mounting & shared folders with parent mac OSx file system
                    ie. tested with direct git clone to /srv/dm4c-fivem (no mounting).. and same yarn issues persist
            note: these yarn issues seem to be stem further issues with resources: 'webpack' and 'chat'
                    ie. resources: 'yarn', 'webpack', & 'chat' ... ALL indeed load fine on remote AWS ubunut-22 server
        - tried mulitple attempts with symlinks from outside mounted paths, etc.
                setting: $ ln -s /var/cache/fivem/server-data/cache /mnt/dm4c-fivem/server-data/cache
            as well setting server.cfg w/ a specific yarn caceh path...
                adding line: set yarn_cache_path "/var/cache/fivem-yarn"
            as well as mount bindings attempts in the same manner (via '/etc/fstab' file updates)
                adding line: /var/cache/fivem/server-data/cache /mnt/dm4c-fivem/server-data/cache none bind 0 0
            NOTE: none of these things seem to be working :/
        - however, the issue doesn't seem to be effecting any current code integration; hence, ignoring it for now 
            as our goal is to stick w/ just lue & c# integrations (w/ simple html/css/js as needed; no node.js stuff)
        - the issue, right now, seems to mainaly effect excess server 'lag' logging, in this manner...
                [ citizen-server-impl] sync thread hitch warning: timer interval of 334 milliseconds
                [ citizen-server-impl] network thread hitch warning: timer interval of 334 milliseconds
            NOTE: current work around... (to mute this excess 'lag' logging)
                run server via: $ .../run.sh +exec server.cfg | grep -v "hitch warning"
        - NOTE: example error logging that stemmed this debug process... 
            ## visible only on local ubunut-22 VM runs (but NOT on remote AWS ubuntu-22 runs)
            ##  offical yarn error logs from fivem directly: .../dm4c-fivem/server-data/resources/[system]/[builders]/webpack
            [    c-scripting-core] Creating script environments for yarn
            [           resources] Started resource yarn
            [ citizen-server-impl] Running build tasks on resource webpack - it'll restart once completed.
            [    c-resources-core] Could not start dependency webpack for resource chat.
            [ citizen-server-impl] Couldn't start resource chat.
            [         script:yarn] [yarn]	[1/4] Resolving packages...
            [         script:yarn] [yarn]	[2/4] Fetching packages...
            [         script:yarn] [yarn]	info fsevents@2.3.1: The platform "linux" is incompatible with this module.
            [         script:yarn] [yarn]	info "fsevents@2.3.1" is an optional dependency and failed compatibility check. Excluding it from installation.
            [         script:yarn] [yarn]	info fsevents@1.2.13: The platform "linux" is incompatible with this module.
            [         script:yarn] [yarn]	info "fsevents@1.2.13" is an optional dependency and failed compatibility check. Excluding it from installation.
            [         script:yarn] [yarn]	[3/4] Linking dependencies...
            [         script:yarn] Error: [yarn]	/mnt/dm4c-fivem/server-data/resources/[system]/[builders]/yarn/yarn_cli.js:89153
            [         script:yarn]     compromised = compromised || function (err) { throw err; };
            [         script:yarn]                                                   ^
            [         script:yarn] 
            [         script:yarn] Error: Unable to update lock within the stale threshold
            [         script:yarn]     at /mnt/dm4c-fivem/server-data/resources/[system]/[builders]/yarn/yarn_cli.js:89075:66
            [         script:yarn]     at FSReqCallback.oncomplete (node:fs:188:23) {
            [         script:yarn]   code: 'ECOMPROMISED'
            [         script:yarn] }
            [ citizen-server-impl] Building resource webpack failed.
            [ citizen-server-impl] Error data: yarn failed!

## build server-data (c# based) and deploy on remote linux server (ubuntu)
    - install .net core on remote server
        # ref: https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#set-environment-variables-system-wide
        # NOTE_SUCCESS: appears to work fine on ubuntu 18.0.x & 22.0.x, but NOT ubuntu 24.0.x (but maybe i messed up and tried 'apt' first like grok suggested below)
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
        $ mkdir ../_server
        $ cd ../_server
        $ wget https://runtime.fivem.net/artifacts/fivem/build_proot_linux/master/14033-60505548e21b6d6e0844e02e571513e15bff5ccc/fx.tar.xz
        $ tar xf fx.tar.xz

    - deploy fivem server (via: run.sh +exec server.cfg)
        $ cd .../server-data
        $ ../_server/run.sh +exec server.cfg
            note: use vanilla server.cfg (placed in .../server-data)
                https://docs.fivem.net/docs/server-manual/setting-up-a-server-vanilla/#linux)
            note: add vanilla lua resources from:
                https://github.com/citizenfx/cfx-server-data.git

    - install GTAV via steam on windows
    - search for running server in servers.fivem.net (via fivem windows client)
    - join game via fivem client on windows
    
## init project & build (local mac osx - unix base .net core)
    $ DOTNET_CLI_TELEMETRY_OPTOUT=1
    $ dotnet workload update
    $ dotnet --version
    $ dotnet new -i CitizenFX.Templates
    $ cd .../dm4c-fivem
    $ mkdir MyResource
    $ cd MyResource/
    $ dotnet new cfx-resource
    $ dotnet build