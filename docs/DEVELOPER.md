# Developer & API notes
## Python / MySql / sqlalchemy
    ref: https://docs.sqlalchemy.org/en/13/dialects/mysql.html#module-sqlalchemy.dialects.mysql.pymysql
        - mysql+pymysql://<username>:<password>@<host>/<dbname>[?<options>]
    ref: https://mariadb.com/resources/blog/how-to-connect-python-programs-to-mariadb/
    ref: https://pymysql.readthedocs.io/en/latest/user/examples.html
        - SQLAlchemy install
            $ python3.7 -m pip install PyMySQL
            
## Python 3.7 install (ubuntu 14.0.4)
    ref: https://tecadmin.net/install-python-3-7-on-ubuntu-linuxmint/
        $ sudo apt-get install build-essential checkinstall
        $ sudo apt-get install libreadline-gplv2-dev libncursesw5-dev libssl-dev libsqlite3-dev tk-dev libgdbm-dev libc6-dev libbz2-dev libffi-dev zlib1g-dev
        $ cd /usr/src
        $ sudo wget https://www.python.org/ftp/python/3.7.3/Python-3.7.3.tgz
        $ sudo tar xzf Python-3.7.3.tgz
        $ cd Python-3.7.3
        $ sudo ./configure --enable-optimizations
        $ sudo make altinstall
        
## web refs
    - Native Mapped locations
        https://www.ign.com/maps/gta-5/los-santos-blaine-county
    - Native Resources (APIs)
        https://runtime.fivem.net/doc/natives/
    - spawn values
        https://pastebin.com/QuDYpGHz
        https://www.se7ensins.com/forums/threads/gta-v-vehicle-hashes-list.988584/
    - spawn pickup values
        https://pastebin.com/8EuSv2r1
    - GTA V non-steam launcher
        https://sourceforge.net/projects/gtavnonsteamlauncher/
    - player trust view Steam ID (requires steam API dev key)
        https://github.com/SaltyGrandpa/FiveM-PlayerTrust
    - GTA online wiki
        https://wiki.gt-mp.net/index.php/InteriorPropList
    - fivem / gtav fix full screen issue
        https://forum.fivem.net/t/wont-go-full-screen/2483
    - update python3.6 on ubuntu 14.0.4
        http://devopspy.com/python/install-python-3-6-ubuntu-lts/
            $ apt-cache search python3.6
            $ sudo add-apt-repository ppa:jonathonf/python-3.6
            $ sudo apt-get update
            $ sudo apt-get install python3.6
        #NOTE: '$ pip3' == '$ python3.6 -m pip'
            $ pip3 install discord.py
            $ python3.6 -m pip install discord.py
        
## fivem natives
    - https://runtime.fivem.net/doc/natives/
    - print
        ref: https://forum.fivem.net/t/print-trace-events/16073/2?u=housesellout
        - to print in the server console use
            print(“Hello World”) 
        - to print in the client console aka F8 console use
            Citizen.Trace(“Hello World”)
            
        ref: https://forum.fivem.net/t/weird-chat-message-issue/37853/6?u=housesellout
        - invokes client side f8 console and chat prints from server side 'print()'
            .../server-data/resources/[system]/chat/cl_chat.lua
            AddEventHandler('__cfx_internal:serverPrint', function(msg)
                -- prints to f8 client console (when calling server side 'print()') 
                print(msg)
                
                -- prints to client side chat (when calling server side 'print()') 
                SendNUIMessage({
                    type = 'ON_MESSAGE',
                    message = {
                        templateId = 'print',
                        multiline = true,
                        args = { msg }
                    }
                })
            end)    
    - death
        ref: .../server-data/resources/[system]/baseevents/server.lua
        AddEventHandler('baseevents:onPlayerKilled', function(killedBy, data)
            local victim = source
        
            RconLog({msgType = 'playerKilled', victim = victim, attacker = killedBy, data = data})
        end)
        
        AddEventHandler('baseevents:onPlayerDied', function(killedBy, pos)
            local victim = source
        
            RconLog({msgType = 'playerDied', victim = victim, attackerType = killedBy, pos = pos})
        end)
        
        
    - IsPedArmed
        -- 0x475768A975D5AD17 0x0BFC892C
        -- IS_PED_ARMED
        local retval --[[ boolean ]] = IsPedArmed(
                                            ped --[[ Ped ]], 
                                            p1 --[[ integer ]]
                                        )
        p1 is anywhere from 4 to 7 in the scripts. Might be a weapon wheel group?  
        ^It's kinda like that.   
        7 returns true if you are equipped with any weapon except your fists.  
        6 returns true if you are equipped with any weapon except melee weapons.  
        5 returns true if you are equipped with any weapon except the Explosives weapon group.  
        4 returns true if you are equipped with any weapon except Explosives weapon group AND melee weapons.  
        3 returns true if you are equipped with either Explosives or Melee weapons (the exact opposite of 4).  
        2 returns true only if you are equipped with any weapon from the Explosives weapon group.  
        1 returns true only if you are equipped with any Melee weapon.  
        0 never returns true.  
        Note: When I say "Explosives weapon group", it does not include the Jerry can and Fire Extinguisher.
        
## console commands
    cfx> refresh; restart dm4c_util; restart dm4c_stamina; restart dm4c_wallet; restart dm4c_store; restart dm4c_ammo; restart dm4c_spawn; restart esx_kashacters; refresh;
    cfx> refresh; restart dm4c_spawn; restart dm4c_util; refresh;
    
    cfx> refresh; restart dm4c_util; restart dm4c_stamina; restart dm4c_wallet; restart dm4c_store; restart dm4c_ammo;
    cfx> refresh; restart dm4c_util; restart dm4c_wallet; restart dm4c_store; restart dm4c_ammo;
    
    cfx> refresh; restart dm4c_util; restart dm4c_wallet; restart dm4c_store; restart dm4c_ammo; restart dm4c_spawn;
    
    cfx> refresh; restart dm4c_spawn; restart esx_ambulancejob;
    
    cfx> refresh; restart dm4c_stamina;
    cfx> refresh; restart dm4c_wallet; 
    cfx> refresh; restart dm4c_store;
    cfx> refresh; restart dm4c_ammo;
    cfx> refresh; restart dm4c_util;
    cfx> refresh; restart dm4c_spawn;
    
## user input commands
    '/play' to set spawn type
        input: /play <type>
        algorithm
            - only 1 game type will be running an active run loop at a time
                the others will be disabled
            - each game has its own resource (i.e. dmc4_ammo, dm4c_tankwars, dm4c_wavewars)
                cmd: '/play <input_resource_game>' 
                    stops the current game resource & starts the input resource
                        -- ref: https://docs.fivem.net/docs/resources/mapmanager/#changemap
                        StopResource(resource_name)
                        StartResource(resource_name)
            - this is all controlled and integrated within...
                dm4c_spawn/server.lua
                    stopALlResources()
                    enableGameType(strType)
                    RegisterCommand('play')
        
    '/gps' to get current coords & paint zone mappings (optional)
        input: /gps
        input: /gps paint <width> <height> <heading> <display>

    '/jump' to 4 pre-set zones, and prints keys table in server logs
        input: /jump <zone_str> <loc_key>
        print keys... (in resources/[dm4c]/dm4c_spawn/config.lua)
            ‘/jump venice’
            ‘/jump tankwars‘
            ‘/jump wavers`‘
            ‘/jump air‘
    
## Lua general
    - '#' operator (ref: https://stackoverflow.com/a/27434198/2298002)
        foo = {}
        foo[#foo+1]="bar"
        foo[#foo+1]="baz"
        This works because the # operator computes the length of the list. The empty list has length 0, etc.

    - append to end of array/list (ref: https://stackoverflow.com/a/27434198/2298002)
        foo = {}
        foo[#foo+1]="bar"
        foo[#foo+1]="baz"
        This works because the # operator computes the length of the list. The empty list has length 0, etc.

    - lua array indecies start with 1, not 0 (ref: https://stackoverflow.com/a/2785854/2298002)
        Lua is descended from Sol, a language designed for petroleum engineers with no formal training in computer programming. People not trained in computing think it is damned weird to start counting at zero. By adopting 1-based array and string indexing, the Lua designers avoided confounding the expectations of their first clients and sponsors.

        Although I too found them weird at the beginning, I have learned to love 0-based arrays. But I get by OK with Lua's 1-based arrays, especially by using Lua's generic for loop and the ipairs operator—I can usually avoid worrying about just how arrays are indexed.

    - arrays (ref: https://www.tutorialspoint.com/lua/lua_arrays.htm)
        --One-Dimensional Array--
        -- Initializing the array
        array = {"Lua", "Tutorial"}

        -- loop through and print array
        for i = 0, 2 do
           print(array[i])
        end


        --Multi-Dimensional Array--
        -- Initializing the array
        array = {}
        for i=1,3 do
           array[i] = {}
            
           for j=1,3 do
              array[i][j] = i*j
           end
            
        end

        -- Accessing the array
        for i=1,3 do
           for j=1,3 do
              print(array[i][j])
           end
        end

    - tables (ref: https://www.tutorialspoint.com/lua/lua_tables.htm)
        --sample table initialization
        mytable = {}

        --simple table value assignment
        mytable[1]= "Lua"

        --removing reference
        mytable = nil

        -- lua garbage collection will take care of releasing memory

    - type()
        ref: https://stackoverflow.com/questions/5250374/using-type-function-to-see-if-current-string-exist-as-table

        ex...
        if type(var_param) == "number"
            return true
        end
        if type(var_param) == "string"
            return true
        end
        if type(var_param) == "table"
            return true
        end


    - cast to number
        local strfloat = "200.0"
        local num0 = tonumber(strfloat)
        
        local strint = "200"
        local num1 = tonumber(strint)
        
    - get current time
        local funcname = os.time() .. '_[dm4c:weaponChanged]'
        
    - exception handling example
        if pcall(foo(bar)) then
            -- no errors while running `foo(bar)'
            while IsPedReloading(playerPed) do
                Citizen.Wait(10)
            end
        else
            local pass = 1
            -- `foo(bar)' raised an error: take appropriate actions
        end
        
## hot / live updates
    update resource (w/ __resources.lua file update)
        > refresh; restart <resource-name>
    
    update resuorce (w/o __resources.lua file update)
        > restart <resource-name>
        
## global keys
    - MP0_STAMINA
    - MP0_STRENGTH
    
## Classes
    - player.lua
        self.setMoney = function(money)
        self.setAccountMoney = function(acc, money)
        self.setJob = function(name, grade)
        self.addWeapon = function(weaponName, ammo)
        self.removeWeapon = function(weaponName, ammo)
        self.hasWeapon = function(weaponName)
        
## ESX
### login / create user (esx_kashacters)
    -ref: resources/[esx]/esx_kashacters/html/ui.html
        <div class="character-buttons">
            <button class="btn btn-play" id="play-char">PLAY</button>
            <button class="btn btn-delete" id="delete" data-toggle="modal" data-target="#delete-char">DELETE</button>
        </div>

    -ref: resources/[esx]/esx_kashacters/html/js/app.js
        $("#play-char").click(function () {
            $.post("http://esx_kashacters/CharacterChosen", JSON.stringify({
                charid: $('.active-char').attr("data-charid"),
                ischar: $('.active-char').attr("data-ischar"),
            }));
            Kashacter.CloseUI();
        });
        
    -ref: resources/[esx]/esx_kashacters/client/main.lua
        RegisterNUICallback("CharacterChosen", function(data, cb)
            SetNuiFocus(false,false)
            DoScreenFadeOut(500)
            TriggerServerEvent('kashactersS:CharacterChosen', data.charid, data.ischar)
            while not IsScreenFadedOut() do
                Citizen.Wait(10)
            end
            cb("ok")
        end)
        
    -ref: resources/[esx]/esx_kashacters/server/main.lua
        RegisterServerEvent("kashactersS:CharacterChosen")
        AddEventHandler('kashactersS:CharacterChosen', function(charid, ischar)
            local src = source
            local spawn = {}
            SetLastCharacter(src, tonumber(charid))
            SetCharToIdentifier(GetPlayerIdentifiers(src)[1], tonumber(charid))
            if ischar == "true" then
                spawn = GetSpawnPos(src)
            else
                TriggerClientEvent('skinchanger:loadDefaultModel', src, true, cb)

                -- DM4C -> RANDOM VENICE SKY-DIVE DEFAULT SPAWN LOCATION
                spawn = exports.dm4c_spawn:getRndmVeniceSkyDiveDefaultSpawnCoords()

                -- spawn = { x = 195.55, y = -933.36, z = 29.90 } -- DEFAULT SPAWN POSITION
            end
            TriggerClientEvent("kashactersC:SpawnCharacter", src, spawn)
        end)
    
### HUD
    -ref: resources/[esx]/es_extended/html/js/app.js
        switch (data.action) {
            case 'setHUDDisplay': {
                ESX.setHUDDisplay(data.opacity);
                break;
            }

            case 'insertHUDElement': {
                ESX.insertHUDElement(data.name, data.index, data.priority, data.html, data.data);
                break;
            }

            case 'updateHUDElement': {
                ESX.updateHUDElement(data.name, data.data);
                break;
            }

            case 'deleteHUDElement': {
                ESX.deleteHUDElement(data.name);
                break;
            }

            case 'inventoryNotification': {
                ESX.inventoryNotification(data.add, data.item, data.count);
            }
        }

### server side
    - shared object 
        ESX = nil
        TriggerEvent('esx:getSharedObject', function(obj) ESX = obj end)
    - player
        local xPlayer = ESX.GetPlayerFromId(_source)
    - weapons
        local weapons = ESX.GetWeaponList()
        local weaponLabel = ESX.GetWeaponLabel(weaponName)
    - save ref: .../server-data/resources/[esx]/es_extended/server/functions.lua
        ESX.SavePlayer = function(xPlayer, cb)
        ESX.SavePlayers = function(cb)

### client side
    - .../server-data/resources/[esx]/es_extended/client/functions.lua
        ESX.ShowAdvancedNotification = function(title, subject, msg, icon, iconType)
        ESX.ShowHelpNotification = function(msg)
        ESX.ShowNotification = function(msg)

    - teleport
        ESX.Game.Teleport(PlayerPedId(), JailLocation)
    
    - sessionmanager/client/empty.lua (note about scheduler.lua):
        --This empty file causes the scheduler.lua to load clientside
        --scheduler.lua when loaded inside the sessionmanager resource currently manages remote callbacks.
        --Without this, callbacks will only work server->client and not client->server.
        
    - .../server-data/resources/[esx]/esx_ambulancejob/client/main.lua
        OnPlayerDeath()

    - emotes .../server-data/resources/radiant_animations/client.lua
        RegisterCommand("e",function(source, args)
        
### server callback
    - server side
        ESX.RegisterServerCallback('dm4c:clipfull', function(source, cb, activeAmmoCnt, currWeapon)
            cb(success, msg, _source)
        end)
        
        - client side (note: client does NOT block until callback is returned)
        ESX.TriggerServerCallback('dm4c:clipfull', function(hasCash, msg, reloadPlayID)
            if hasCash then
                showHelpNotification("hasCash is true", 3)
            else
                showHelpNotification(msg, 3)
            end
        end, activeAmmoCnt, currWeapon)

## MySql queries
### show users' name, steamID, cash (money), bank, ammo (black money)
    - display: steamID for user name
    mysql> select name, identifier from users where name = 'house';
    
    - fix: ammo/black_money stuck issue in '/play deathmatch' (clear user's "black_money" in db)
    mysql> update user_accounts set money = 0 where identifier = 'steam:11000013b44adb1' and name = 'black_money';
    
    - display: all from user_accounts table (cols: id, identifier, name->'black_money', money)
    mysql> select * from user_accounts where identifier = 'steam:11000013b44adb1' and name = 'black_money';
    
    - display: name, steamID, cash, bank, ammo_cash, 
    mysql> select users.name,users.identifier, users.money, bank, user_accounts.money as ammo from users, user_accounts where users.identifier = user_accounts.identifier;

    - display: name, steamID, cash, bank, ammo_cash, is_dead
    mysql> select users.name,users.identifier, users.money, bank, user_accounts.money as ammo, is_dead from users, user_accounts where users.identifier = user_accounts.identifier;
    
    - display: name, steamID, cash, bank, ammo_cash, is_dead, last_property
    mysql> select users.name,users.identifier, users.money, bank, user_accounts.money as ammo, last_property as last_ammo, is_dead from users, user_accounts where users.identifier = user_accounts.identifier;
    
    - display: name, firstname, lastname, steamID, cash, bank, ammo_cash, is_dead, last_property
    mysql> select users.name,users.firstname,users.lastname,users.identifier, users.money, bank, user_accounts.money as ammo, last_property as last_ammo, is_dead from users, user_accounts where users.identifier = user_accounts.identifier;

## MySql in lua (server side)
    ex: mysql call from lua (select statement))
        MySQL.Async.fetchAll('SELECT * FROM `jobs` WHERE `name` = @name', {
            ['@name'] = name
        }, function(result)

            self.job['id']    = result[1].id
            self.job['name']  = result[1].name
            self.job['label'] = result[1].label
        end)
        
    ex: mysql call from lua (update statement)
        local xPlayer = ESX.GetPlayerFromId(source)
        MySQL.Async.execute('UPDATE users SET last_property = @last_property WHERE identifier = @identifier',
            {
                ['@last_property'] = property,
                ['@identifier']    = xPlayer.identifier
            })

## client side native/global functions
### unsorted
    -- APPLY_FORCE_TO_ENTITY
        ref: https://docs.fivem.net/natives/?_0xC5F68BE9613E2D18
        ref: https://gtaforums.com/topic/885669-precisely-define-object-physics/
        ref: https://gtaforums.com/topic/887362-apply-forces-and-momentums-to-entityobject/
        ref: https://github.com/ethanfs20/fivem_gravgun
            example:
                ApplyForceToEntity(heldEntity, 1, downForce, 0.0, 0.0,
                                               0.0, false, false, true, true, false,
                                               true)
        ApplyForceToEntity(
            entity --[[ Entity ]], 
            forceType --[[ integer ]], 
            x --[[ number ]], 
            y --[[ number ]], 
            z --[[ number ]], 
            offX --[[ number ]], 
            offY --[[ number ]], 
            offZ --[[ number ]], 
            boneIndex --[[ integer ]], 
            isDirectionRel --[[ boolean ]], 
            ignoreUpVec --[[ boolean ]], 
            isForceRel --[[ boolean ]], 
            p12 --[[ boolean ]], 
            p13 --[[ boolean ]]
        )
        
        Parameters:
            entity: The entity you want to apply a force on
            forceType: See native description above for a list of commonly used values
            x: Force amount (X)
            y: Force amount (Y)
            z: Force amount (Z)
            offX: Rotation/offset force (X)
            offY: Rotation/offset force (Y)
            offZ: Rotation/offset force (Z)
            boneIndex: (Often 0) Entity bone index
            isDirectionRel: (Usually false) Vector defined in local (body-fixed) coordinate frame
            ignoreUpVec: (Usually true)
            isForceRel: (Usually true) When true, force gets multiplied with the objects mass and different objects will have the same acceleration
            p12: (Usually false)
            p13: (Usually true)
            
        List of force types (p1):
            public enum ForceType
            {
                MinForce = 0,
                MaxForceRot = 1,
                MinForce2 = 2,
                MaxForceRot2 = 3,
                ForceNoRot = 4,
                ForceRotPlusForce = 5
            }
            
        Examples (lua):
            local forceTypes = {
                MinForce = 0,
                MaxForceRot = 1,
                MinForce2 = 2,
                MaxForceRot2 = 3,
                ForceNoRot = 4,
                ForceRotPlusForce = 5
            }

            local entity = PlayerPedId()
            local forceType = forceTypes.MaxForceRot2
             -- sends the entity straight up into the sky:
            local direction = vector3(0.0, 0.0, 15.0)
            local rotation = vector3(0.0, 0.0, 0.0)
            local boneIndex = 0
            local isDirectionRel = false
            local ignoreUpVec = true
            local isForceRel = true
            local p12 = false
            local p13 = true

            ApplyForceToEntity(
                entity,
                forceType,
                direction,
                rotation,
                boneIndex,
                isDirectionRel,
                ignoreUpVec,
                isForceRel,
                p12,
                p13
            )            
    
    -- SET_PED_STUFF
        Citizen.CreateThread(function()
            while true do
                SetBlockingOfNonTemporaryEvents(pedNpc,true)
                SetPedFleeAttributes(pedNpc, 0, 0)
                SetPedCombatAttributes(pedNpc, 17, 1)
                if(GetPedAlertness(pedNpc) ~= 0) then
                    SetPedAlertness(pedNpc,0)
                end
            end
        end)
        
    -- SET_TRAFFIC_DENSITY
        local density = 0.45 -- Anything between 0.0 and 1.0 is a valid density, anything lower/higher is pointless
        Citizen.CreateThread(function()
            while true do
                Citizen.Wait(0)
                SetVehicleDensityMultiplierThisFrame(density)
                SetPedDensityMultiplierThisFrame(density)
                SetRandomVehicleDensityMultiplierThisFrame(density)
                SetParkedVehicleDensityMultiplierThisFrame(density)
                SetScenarioPedDensityMultiplierThisFrame(density, density)
            end
        end)
        ref: https://forum.cfx.re/t/increase-traffic-density/4844102
        
    -- setting npc cars high speed attempts...
        -- refs:
            https://pastebin.com/SsFej963
            https://www.oreilly.com/library/view/lua-quick-start/9781789343229/1bd45cc8-16ba-423a-a4db-247a6b53ddc8.xhtml
            https://cpp.hotexamples.com/examples/-/LuaRef/-/cpp-luaref-class-examples.html
            https://stackoverflow.com/questions/16479716/how-to-access-c-pointers-in-lua?rq=3
            https://stackoverflow.com/questions/18261891/how-to-extract-c-object-pointer-from-lua?rq=3
            https://forum.cfx.re/t/getpednearbyvehicles-peds/2095
            https://docs.fivem.net/natives/?_0xCFF869CBFA210D82
            https://docs.fivem.net/natives/?_0xBAA045B4E42F3C06
            
        -- _SET_VEHICLE_MAX_SPEED
            SetVehicleMaxSpeed(
                vehicle --[[ Vehicle ]], 
                speed --[[ number ]]
            )
            Parameters:
            vehicle: The vehicle handle.
            speed: The speed limit in meters per second.
            To reset the max speed, set the speed value to 0.0 or lower.
            
        -- SET_VEHICLE_GRAVITY
            SetVehicleGravity(
                vehicle --[[ Vehicle ]], 
                toggle --[[ boolean ]]
            )
        
        -- GET_NEARBY_VEHICLES
            local retval, nearbyVehicles = GetPedNearbyVehicles(playerPed) -- returns a 'number' type
            
        -- _GET_ALL_VEHICLES
            local retval --[[ integer ]], vehArray --[[ integer ]] = GetAllVehicles()
            
        -- SET_VEHICLE_FORWARD_SPEED
            SetVehicleForwardSpeed(
                vehicle --[[ Vehicle ]], 
                speed --[[ number ]]
            )
            SCALE: Setting the speed to 30 would result in a speed of roughly 60mph, according to speedometer.  
            Speed is in meters per second  
            You can convert meters/s to mph here:  
            http://www.calculateme.com/Speed/MetersperSecond/ToMilesperHour.htm  
        
    -- blips (ref: esx_eden_garage/client.lua)
        local blip = AddBlipForCoord(zoneValues.Pos.x, zoneValues.Pos.y, zoneValues.Pos.z)
        SetBlipSprite (blip, Config.BlipInfos.Sprite)
        SetBlipDisplay(blip, 4)
        SetBlipScale  (blip, 1.0)
        SetBlipColour (blip, Config.BlipInfos.Color)
        SetBlipAsShortRange(blip, true)
        BeginTextCommandSetBlipName("STRING")
        AddTextComponentString("Garage")
        EndTextCommandSetBlipName(blip)

    SetCamActive(cam, false)
    DestroyCam(cam, true)
    IsChoosing = false
    DisplayHud(true)
    DisplayRadar(true)
    DoScreenFadeIn(500)
    DisablePlayerVehicleRewards(PlayerId())
    ClearPedBloodDamage(ped)
    SetPlayerInvincible(ped, false)
    success, vec3 = GetSafeCoordForPed(vector.x, vector.y, vector.z, false, 28)
    SetPedAmmo(playerPed, weaponHash, 0) -- remove leftover ammo
    GetHashKey(weaponName)
    PlayerId()
    GetPlayerWantedLevel(playerId)
    SetPlayerWantedLevel(playerId, 0, false)
    SetPlayerWantedLevelNow(playerId, false)
    IsPlayerInCutscene(PlayerId())
    SetBlipNameToPlayerName(
        blip --[[ Blip ]], 
        player --[[ Player ]]
    )
    IsGameplayCamRendering()
    SetPlayerAngry(playerPed, false)
    SetPedDropsWeaponsWhenDead(playerPed, true)
    local vehicle = GetClosestVehicle(coords.x,coords.y,coords.z,radius,modelHash,flags)
        coords = GetEntityCoords(playerPed)
        radius = 2.0
        modelHash = lastModelHash
        flags = 70 
            -- ref: https://forum.fivem.net/t/detecting-vehicles/850/12?u=housesellout
            -- ref: https://forum.fivem.net/t/help-how-to-return-a-boat-getclosestvehicle/22479/2?u=housesellout
            00000000000000000 = 0 - Only returns cars and motorbikes. Often returns vehicle already inside. Otherwise only return empty vehicles.  
            00000000000000010 = 2 - Only empty vehicles. Only returns cars and motorbikes.
            00000000000000100 = 4 - Works like 70. During the testing, this one worked the best i.e. for cars and motorbikes
            00000000000000110 = 6 - Works like 70.
            00000000000000111 = 7 - Works like 70.
            00000000000010111 = 23 - Only finds cars when not inside one.
            00000000001000110 = 70 - (Not from scripts. Recommended by the native db). Only works with motorbikes and cars.    
            00000000001111111 = 127 - While inside cars or motorbikes, nothing can be found. On foot or inside heli cars can be found.
            00000000100000100 = 260 - Works like 70.
            00000100001100010 = 2146
            00000100001111111 = 2175
            00011000000000110 = 12294
            00100000000000000 = 16384
            00100000000000010 = 16386
            00101000000010111 = 20503
            01000000000000000 = 32768
            10000100000000110 = 67590
            10000100001111111 = 67711 - Finds cars when inside heli, not when inside cars.
            11000000000000101 = 98309
            11000100000000111 = 100359 - Works like 70 but returns the vehicle the player already is in quite often.
            11111111111111111 = 131071 - Not from the scripts. Nothing seems to work when testing.
            00100000000000000 = 16384 - returns planes only.
    DeleteVehicle(vehicle)
    IsPedInParachuteFreeFall(playerPed)
    local intState = GetPedParachuteState(playerPed)
        Returns:  
        -1: Normal  
        0: Wearing parachute on back  
        1: Parachute opening  
        2: Parachute open  
        3: Falling to doom (e.g. after exiting parachute)  
        Normal means no parachute?  
    local playerVehicle = GetVehiclePedIsIn(playerPed, false)
        -- false = CurrentVehicle, true = LastVehicle
    local currVehicleHP = GetVehicleBodyHealth(playerVehicle) -- min 0, max 1000
    SetWeatherTypeNow(weatherType --[[ string ]])
    SetWeatherTypeNowPersist(weatherType --[[ string ]])
        The following weatherTypes are used in the scripts:
        "CLEAR"
        "EXTRASUNNY"
        "CLOUDS"
        "OVERCAST"
        "RAIN"
        "CLEARING"
        "THUNDER"
        "SMOG"
        "FOGGY"
        "XMAS"
        "SNOWLIGHT"
        "BLIZZARD"
    SetPedCombatMovement(newNpcPed,combatMovement)
        0 - Stationary (Will just stand in place)
        1 - Defensive (Will try to find cover and very likely to blind fire)
        2 - Offensive (Will attempt to charge at enemy but take cover as well)
        3 - Suicidal Offensive (Will try to flank enemy in a suicidal attack)
        
### globalizing functions (export functions)
    - NOTE: exporting from a resource, does not require that resource to be added as a
             dependency in another resource to be used
    - add 'export' command to __resource.lua of function you want to be global
        __resource.lua in dm4c_wallet
            export 'cash2ammo' -- for client_scripts
            server_export 'cash2ammo' -- for server_scripts

    - invoke function from another .lua file
        server.lua in dm4c_ammo
            exports.dm4c_wallet:cash2ammo(_source, activeAmmoCnt, xPlayer)
            
### player id 
    - get dynamic server side player ID (displayed in lower left corner)
        _source == GetPlayerServerId(PlayerId())
    - get client side player ID from server side event handler 'source'
        PlayerId() == GetPlayerFromServerId(_source)

### notification
    #ref: https://forum.fivem.net/t/common-ways-to-display-help-text/292363
        function showHelpNotification(text, durSec)
            BeginTextCommandDisplayHelp("STRING")
            AddTextComponentSubstringPlayerName(text)

            -- shape (always 0), loop (bool), makeSound (bool), duration (0 for loop)
            -- EndTextCommandDisplayHelp(0, 1, 1, 0)

            -- shape (always 0), loop (bool), makeSound (bool), duration (5000 max 5 sec)
            EndTextCommandDisplayHelp(0, 0, 1, durSec * 1000)
            
            -- shape (always 0), loop (bool), makeSound (bool), duration (-1 for 10sec i think)
            -- EndTextCommandDisplayHelp(0, 0, 1, -1)
        end

### coordinates & spawning
    - check if coordinate is available
    ex:
        RequestCollisionAtCoord(coords.x, coords.y, coords.z)
        while not HasCollisionLoadedAroundEntity(entity) do
            RequestCollisionAtCoord(coords.x, coords.y, coords.z)
            Citizen.Wait(0)
        end
        SetEntityCoords(entity, coords.x, coords.y, coords.z)
    
### ESX.Game.GetClosestPlayer()
    ex:
        local closestPlayer, closestDistance = ESX.Game.GetClosestPlayer()
        
### IsPedArmed()
    - checks is player (or ped) is armed
        pass 7 to include slapper weapons
        pass 4 to exclude slapper weapons
    ex:
        local armed = IsPedArmed(PlayerPedId(), 7)
        local armed = IsPedArmed(PlayerPedId(), 4)

### TriggerClientEvent()
    - set cash display
        TriggerClientEvent('es:activateMoney', self.source , self.money)
    - set bank display
        TriggerClientEvent('esx:setAccountMoney', self.source, account)
        
### StatSetInt()
    - set user status
        StatSetInt("MP0_STAMINA", 100, true)
        StatSetInt("MP0_STRENGTH", 100, true)
        
## server side global functions
### StartResource / StopResource
    -- ref: https://docs.fivem.net/docs/resources/mapmanager/#changemap
    function changeMap(map)
        if currentMap then
            StopResource(currentMap)
        end

        StartResource(map)
    end
    
### RegisterCommand()
    - register command instruction w/ function to run
    ex:
        RegisterCommand('setadmin', function(source, args, raw)
        
### DropPlayer()
    - server side function
    ex:
        DropPlayer(source, _U('afk_kicked_message'))
        
### PlayerId()
    - gets 'this' player's id (validated from client.lua)
    - PlayerId() == source (on server side event handler)
    ex: 
        local player = PlayerId()
   
## event handlers
### RegisterCommand()
    ex:
        RegisterCommand("anonym", function(source, args, raw)
            local msg = table.concat(args, ' ')
            TriggerClientEvent('chatMessage', -1, "^2Anonymous Message:", { 255, 255, 255 }, msg)
        end, false)

### TriggerServerEvent
    ex:
        TriggerServerEvent('ResetStamina')

### TriggerClientEvent
    ex:
        TriggerClientEvent('esx:removeWeapon', self.source, weaponName, ammo)

### TriggerEvent()
    ex:
        TriggerEvent('esx:getSharedObject', function(obj) ESX = obj end)

### RegisterClientEvent
    ex:
        RegisterNetEvent('esx:removeWeapon')

### RegisterServerEvent
    ex:
        RegisterServerEvent('ResetStamina')
    note: may receive error 'event 'ResetStamina' was not safe for net'
        - if you don't RegisterServerEvent
        
### AddEventHandler 
    - 'source' param in event handler function seems to be auto-included
    - 'source' == PlayerId()
    ex:
        AddEventHandler('es:playerLoaded', function(source, _player)
            local _source = source
            playerName   = GetPlayerName(_source)
    ex:
        AddEventHandler('es:playerLoaded', function(_player)
            local _source = source
            playerName   = GetPlayerName(_source)
        
    ex: 
        AddEventHandler('es:firstJoinProper', function()
            local Source = source
    ex:
        -- ref: ref: https://docs.fivem.net/docs/scripting-reference/events/list/onResourceStop/#lua-example
        AddEventHandler('onResourceStop', function(resourceName)
          if (GetCurrentResourceName() ~= resourceName) then
            return
          end
          print('The resource ' .. resourceName .. ' was stopped.')
        end)

## mid-air /gps coords
[1557875828 gps][2] CURRENT GPS: {x = -1126.8367919922, y = -1325.073852539, z = 5.2550110816956}

[1557875989 gps][1] CURRENT GPS: {x = -1022.7122192382, y = -1312.993774414, z = 106.58131408692}
[1557876086 gps][2] CURRENT GPS: {x = -171.38845825196, y = -844.44146728516, z = 949.88934326172}

[1557876159 gps][2] CURRENT GPS: {x = -966.74725341796, y = -873.12329101562, z = 1209.9366455078}

[1557876168 gps][1] CURRENT GPS: {x = -1088.7420654296, y = -882.79608154296, z = 955.98266601562}
[1557876171 gps][1] CURRENT GPS: {x = -1091.0366210938, y = -876.01550292968, z = 891.66094970704}

[1557876176 gps][2] CURRENT GPS: {x = -945.14569091796, y = -939.29577636718, z = 722.42193603516}
[1557876180 gps][1] CURRENT GPS: {x = -1066.4953613282, y = -882.7719116211, z = 494.6128540039}
[1557876183 gps][1] CURRENT GPS: {x = -1015.9165039062, y = -848.6752319336, z = 359.12158203125}
[1557876206 gps][2] CURRENT GPS: {x = -574.02899169922, y = -952.97137451172, z = 393.60360717774}

[1557876515 gps][2] CURRENT GPS: {x = -1692.4022216796, y = -1068.1123046875, z = 1517.0888671875}

[1557876549 gps][2] CURRENT GPS: {x = -1408.3129882812, y = -1065.1821289062, z = 958.95379638672}
[1557876585 gps][2] CURRENT GPS: {x = -1270.2337646484, y = -1536.7869873046, z = 705.96697998046}
[1557876618 gps][2] CURRENT GPS: {x = -1316.5837402344, y = -1582.4014892578, z = 558.0853881836}
[1557876638 gps][2] CURRENT GPS: {x = -1252.374633789, y = -1310.0692138672, z = 464.71365356446}
[1557876655 gps][2] CURRENT GPS: {x = -1076.8654785156, y = -1137.6267089844, z = 348.22564697266}
[1557876665 gps][2] CURRENT GPS: {x = -915.49035644532, y = -1056.4127197266, z = 303.63766479492}
[1557876687 gps][2] CURRENT GPS: {x = -591.26379394532, y = -997.51177978516, z = 193.8314819336}
[1557876695 gps][2] CURRENT GPS: {x = -491.26034545898, y = -1010.7461547852, z = 154.91270446778}
[1557876706 gps][2] CURRENT GPS: {x = -340.89874267578, y = -1006.5526123046, z = 101.10076904296}
[1557876713 gps][2] CURRENT GPS: {x = -243.61392211914, y = -1026.8449707032, z = 69.291801452636}

[1557877322 gps][2] CURRENT GPS: {x = -690.22662353516, y = -466.0432434082, z = 977.06341552734}
[1557877336 gps][2] CURRENT GPS: {x = -821.86791992188, y = -309.21719360352, z = 914.15856933594}
[1557877370 gps][2] CURRENT GPS: {x = -888.05041503906, y = 7.0877676010132, z = 756.70556640625}
[1557877395 gps][2] CURRENT GPS: {x = -538.06463623046, y = 82.461128234864, z = 634.75512695312}
[1557877431 gps][2] CURRENT GPS: {x = -56.803043365478, y = 187.81939697266, z = 472.0386352539}
[1557877455 gps][2] CURRENT GPS: {x = 287.53518676758, y = 263.20767211914, z = 355.68991088868}
[1557877494 gps][2] CURRENT GPS: {x = 311.85949707032, y = -190.55319213868, z = 174.23927307128}

[1557877506 gps][2] CURRENT GPS: {x = 144.52592468262, y = -196.65585327148, z = 121.859085083}


