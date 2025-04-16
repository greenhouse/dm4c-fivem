# ref: legeacy -> fivem-dm4c -> [dm4c]/dm4c_spawn/config.lua

Config                            = {}

activeSpawnLocCoords = {}
activeSpawnLocSkyCoords = {}

Config.Zone = {}
Config.Location = {}
Config.PercentSkySpawn = 10
Config.EnableRdmSkySpawn = true

kGameTypeDeathMatch = 'deathmatch'
kGameTypeTankWars = 'tankwars'
kGameTypeWaveWars = 'wavewars'
kGameTypeRC = 'rc'

spawnCoordType = ''
------------------------------
-- SPAWN TYPE FUNC SUPPORT  --
------------------------------

--[==[
    -- NOTE_101320: not in use
    function isActiveSpawnType(strType)
        local funcname = '[isActiveSpawnType]'
        TriggerServerEvent("LogToServer", string.format("%s _ ENTER", funcname))
        local isCorrect = spawnCoordType == strType
        TriggerServerEvent("LogToServer", string.format("%s _ EXIT _ isCorrect: %s", funcname, isCorrect))
        return isCorrect
    end

    -- NOTE_101320: not in use
    function getActiveSpawnType()
        local funcname = '[getActiveSpawnType]'
        TriggerServerEvent("LogToServer", string.format("%s _ ENTER", funcname))
        
        local strReturn = 'nil'
        if spawnCoordType == '' then
            local strprint = string.format("%s _ 'spawnCoordType' is empty; returning 'none; active spawn type'", funcname)
            TriggerServerEvent("LogToServer", strprint)
            print(strprint)
            strReturn = 'none; active spawn type'
        else
            local strprint = string.format("%s _ found 'spawnCoordType' = %s", funcname, spawnCoordType)
            TriggerServerEvent("LogToServer", strprint)
            print(strprint)
            strReturn = spawnCoordType
        end

        TriggerServerEvent("LogToServer", string.format("%s _ EXIT", funcname))
        return strReturn
    end
--]==]

--invoked by tankwars and wavewars (both client.lua & server.lua)
-- client init load -> trigger server invoke -> trigger client callback invoke
--   client.lua: runs on user load and triggers servers side to invoke 'setActiveSpawnLocCoordType'
--   server.lua: then invokes client side callback to invoke 'setActiveSpawnLocCoordType'
function setActiveSpawnLocCoordType(strType)
    local funcname = '[setActiveSpawnLocCoordType]'
    print(string.format("%s _ ENTER strType: %s", funcname, strType))
    spawnCoordType = strType
    print(string.format("%s _ EXIT strType: %s", funcname, strType))
end

function getActiveSpawnLocCoords()
    local funcname = '== [getActiveSpawnLocCoords]'
    -- print(string.format("%s ENTER _ spawnCoordType: %s", funcname, spawnCoordType))

    if spawnCoordType == 'wavewars' then
        return Config.Location.VeniceBeachWaveWars
    end

    if spawnCoordType == 'tankwars' then
        return Config.Location.VeniceBeachTankWars
    end

    -- DEFAULT TO VENICE BEACH OCEAN SKY DIVE
    return Config.Location.VeniceBeachDeathMatch
end

function getActiveSpawnLocSkyCoords()
    local funcname = '== [getActiveSpawnLocSkyCoords]'
    -- print(string.format("%s ENTER _ spawnCoordType: %s", funcname, spawnCoordType))

    if spawnCoordType == 'wavewars' then
        return Config.Location.VeniceBeachSkyWaveWars
    end

    if spawnCoordType == 'tankwars' then
        return Config.Location.VeniceBeachSkyTanksWars
    end

    -- DEFAULT TO VENICE BEACH OCEAN SKY DIVE
    return Config.Location.VeniceBeachSkyDefault
end

function getRndmVeniceSkyDiveDefaultSpawnCoords()
    local funcname = '== [getRndmVeniceSkyDiveDefaultSpawnCoords]'
    -- print(string.format("%s ENTER", funcname))

    -- spawn = {x = -1380.7938232422, y = -1385.1623535156, z = 1800.5787341594696} -- SKPK10
    spawn = {x = -1497.9467773438, y = -1484.6145019532, z = 1800.3762207032} -- CW3SKY
    return spawn
end

------------------------------
-- WAVE WARS SKY-DIVE       --
------------------------------
Config.Location.SkyCount = 7
Config.Location.VeniceBeachSkyWaveWars = {
    -- NORTH WEST 3 SKY-DIVE
    WWARS10 = {x = -1727.2117919922, y = -1278.392578125, z = 800.993303179741},
    WWARS11 = {x = -1767.869140625, y = -1249.3599853516, z = 900.3829925060272},
    WWARS12 = {x = -1821.0056152344, y = -1228.2135009766, z = 1000.895803809166},
    WWARS13 = {x = -1758.8416748046, y = -1166.845703125, z = 1100.0535771846772},
    WWARS14 = {x = -1731.8248291016, y = -1123.8580322266, z = 1000.968689441681},
    WWARS15 = {x = -1707.2028808594, y = -1126.1903076172, z = 900.7843618392944},
    WWARS16 = {x = -1685.9447021484, y = -1146.2373046875, z = 800.9892542362214}
}
------------------------------
-- WAVE WARS                --
------------------------------
Config.Location.Count = 17
Config.Location.VeniceBeachWaveWars = {
    WWARS1 = {x = -1695.4764404296, y = -1115.2674560546, z = 0.66201078891754},
    WWARS2 = {x = -1672.0076904296, y = -1116.0336914062, z = 0.10215941816568},
    WWARS3 = {x = -1655.6281738282, y = -1133.3188476562, z = -0.008433185517788},
    WWARS4 = {x = -1613.484008789, y = -1194.2677001954, z = 0.95112788677216},
    WWARS5 = {x = -1581.5350341796, y = -1242.5212402344, z = 0.72421443462372},
    WWARS6 = {x = -1561.8491210938, y = -1296.6176757812, z = 0.62570875883102},
    WWARS7 = {x = -1562.2043457032, y = -1330.7255859375, z = 0.85472083091736},
    WWARS8 = {x = -1603.0743408204, y = -1337.343383789, z = 1.0220563411712},
    WWARS9 = {x = -1662.8553466796, y = -1309.7470703125, z = -0.021616145968438},
    WWARS10 = {x = -1727.2117919922, y = -1278.392578125, z = 0.39921790361404},
    WWARS11 = {x = -1767.869140625, y = -1249.3599853516, z = -0.48765033483506},
    WWARS12 = {x = -1821.0056152344, y = -1228.2135009766, z = 0.43009316921234},
    WWARS13 = {x = -1758.8416748046, y = -1166.845703125, z = 0.94573783874512},
    WWARS14 = {x = -1731.8248291016, y = -1123.8580322266, z = 0.4144460260868},
    WWARS15 = {x = -1707.2028808594, y = -1126.1903076172, z = 0.48070615530014},
    WWARS16 = {x = -1685.9447021484, y = -1146.2373046875, z = 0.03480550646782},
    WWARS17 = {x = -1694.48046875, y = -1199.1302490234, z = 0.60808277130126}
}


------------------------------
-- TANK WARS SKY-DIVE       --
------------------------------
Config.Location.SkyCount = 7
Config.Location.VeniceBeachSkyTanksWars = {
    -- NORTH WEST 3 SKY-DIVE
    TWARS2 = {x = -1387.4786376954, y = -1255.5740966796, z = 800.993303179741},
    TWARS3 = {x = -1408.0532226562, y = -1089.4267578125, z = 900.3829925060272},
    TWARS4 = {x = -1474.4545898438, y = -1484.2102050782, z = 1000.895803809166},
    TWARS5 = {x = -1314.9172363282, y = -1434.2076416016, z = 1100.0535771846772},
    TWARS6 = {x = -1392.8825683594, y = -1626.5924072266, z = 1000.968689441681},
    TWARS7 = {x = -1371.6577148438, y = -1683.7159423828, z = 900.7843618392944},
    TWARS9 = {x = -1222.5600585938, y = -1826.0434570312, z = 800.9892542362214}
}
------------------------------
-- TANK WARS                --
------------------------------
Config.Location.Count = 9
Config.Location.VeniceBeachTankWars = {
    TWARS1 = {x = -1514.271118164, y = -1103.66796875, z = 2.5111973285676},
    TWARS2 = {x = -1387.4786376954, y = -1255.5740966796, z = 4.3694500923156},
    TWARS3 = {x = -1408.0532226562, y = -1089.4267578125, z = 3.6931533813476},
    TWARS4 = {x = -1474.4545898438, y = -1484.2102050782, z = 2.099172592163},
    TWARS5 = {x = -1314.9172363282, y = -1434.2076416016, z = 4.7438464164734},
    TWARS6 = {x = -1392.8825683594, y = -1626.5924072266, z = 1.7018637657166},
    -- TWARS7 = {x = -1371.6577148438, y = -1683.7159423828, z = 0.53731191158294},
    TWARS7 = {x = -1150.0590820312, y = -1705.0975341796, z = 4.2319869995118},
    TWARS8 = {x = -1150.0590820312, y = -1705.0975341796, z = 4.2319869995118},
    TWARS9 = {x = -1222.5600585938, y = -1826.0434570312, z = 2.5317974090576}
}

------------------------------
-- DEFAULT SKY-DIVE         --
------------------------------
Config.Location.SkyCount = 7
Config.Location.VeniceBeachSkyDefault = {
    -- NORTH WEST 3 SKY-DIVE
    DEFSKY1 = {x = -1727.2117919922, y = -1278.392578125, z = 800.993303179741},
    DEFSKY2 = {x = -1767.869140625, y = -1249.3599853516, z = 900.3829925060272},
    DEFSKY3 = {x = -1821.0056152344, y = -1228.2135009766, z = 1000.895803809166},
    DEFSKY4 = {x = -1758.8416748046, y = -1166.845703125, z = 1100.0535771846772},
    DEFSKY5 = {x = -1731.8248291016, y = -1123.8580322266, z = 1000.968689441681},
    DEFSKY6 = {x = -1707.2028808594, y = -1126.1903076172, z = 900.7843618392944},
    DEFSKY7 = {x = -1685.9447021484, y = -1146.2373046875, z = 800.9892542362214}
}

------------------------------
-- LOCATION SPAWN TYPES     --
------------------------------
Config.Location.SpawnType = {
    {
        Type = 'deathmatch',
        Coords = Config.Location.VeniceBeachWaveWars,
        SkyCoords = Config.Location.VeniceBeachSkyWaveWars
    },
    {
        Type = 'tankwars',
        Coords = Config.Location.VeniceBeachTankWars,
        SkyCoords = Config.Location.VeniceBeachSkyTanksWars
    },
    {
        Type = 'skatepark',
        Coords = Config.Location.VeniceBeachWaveWars,
        SkyCoords = Config.Location.VeniceBeachSkyWaveWars
    },
    {
        Type = 'wavewars',
        Coords = Config.Location.VeniceBeachWaveWars,
        SkyCoords = Config.Location.VeniceBeachSkyWaveWars
    }
}

--[==[
------------------------------
-- SKATE PARK SKY-DIVE      --
------------------------------
Config.Location.SkyCount = 7
Config.Location.VeniceBeachSky = {
    -- NORTH WEST 3 SKY-DIVE
    SKPK1 = {x = -1389.8489990234, y = -1401.8962402344, z = 800.993303179741},
    SKPK2 = {x = -1388.1328125, y = -1405.462890625, z = 900.3829925060272},
    SKPK3 = {x = -1381.506225586, y = -1404.361328125, z = 1000.895803809166},
    SKPK4 = {x = -1379.5063476562, y = -1402.1837158204, z = 1100.0535771846772},
    SKPK5 = {x = -1375.6977539062, y = -1401.8505859375, z = 1000.968689441681},
    SKPK6 = {x = -1386.7055664062, y = -1393.4426269532, z = 900.7843618392944},
    SKPK7 = {x = -1378.8939208984, y = -1395.251953125, z = 800.9892542362214}
}
------------------------------
-- SKATE PARK               --
------------------------------
Config.Location.Count = 19
Config.Location.VeniceBeach = {
    SKPK1 = {x = -1389.8489990234, y = -1401.8962402344, z = 1.993303179741},
    SKPK2 = {x = -1388.1328125, y = -1405.462890625, z = 2.3829925060272},
    SKPK3 = {x = -1381.506225586, y = -1404.361328125, z = 1.895803809166},
    SKPK4 = {x = -1379.5063476562, y = -1402.1837158204, z = 2.0535771846772},
    SKPK5 = {x = -1375.6977539062, y = -1401.8505859375, z = 4.968689441681},
    SKPK6 = {x = -1386.7055664062, y = -1393.4426269532, z = 4.7843618392944},
    SKPK7 = {x = -1378.8939208984, y = -1395.251953125, z = 2.9892542362214},
    SKPK8 = {x = -1385.292602539, y = -1387.613647461, z = 2.317272901535},
    SKPK9 = {x = -1384.4991455078, y = -1380.9239501954, z = 2.8517887592316},
    SKPK10 = {x = -1380.7938232422, y = -1385.1623535156, z = 2.5787341594696},
    SKPK11 = {x = -1375.2305908204, y = -1382.9969482422, z = 2.3704686164856},
    SKPK12 = {x = -1364.9150390625, y = -1384.580078125, z = 2.330708026886},
    SKPK13 = {x = -1358.5144042968, y = -1396.0302734375, z = 2.5070087909698},
    SKPK14 = {x = -1370.9732666016, y = -1408.364868164, z = 2.339349269867},
    SKPK15 = {x = -1358.578125, y = -1386.8200683594, z = 2.3251547813416},
    SKPK16 = {x = -1370.1221923828, y = -1392.9565429688, z = 4.0721325874328},
    SKPK17 = {x = -1371.4301757812, y = -1397.0212402344, z = 3.2867486476898},
    SKPK18 = {x = -1373.4406738282, y = -1400.7584228516, z = 4.0648398399354},
    SKPK19 = {x = -1369.5267333984, y = -1397.685546875, z = 3.3305711746216}
}
--]==]






------------------------------
-- DEATH MATCH VENICE BEACH --
------------------------------
Config.Location.Count = 7
Config.Location.VeniceBeachDeathMatch = {
    N3W3 = {x = -1485.9119873046, y = -1128.9865722656, z = 0.036846715956926}, -- DONE
    N2W3 = {x = -1517.623046875, y = -1273.4562988282, z = 3.459242105484}, -- DONE
    N1W3 = {x = -1533.3830566406, y = -1369.7990722656, z = 0.35384374856948}, -- DONE
    CW3 = {x = -1497.9467773438, y = -1484.6145019532, z = 5.6431970596314}, -- DONE
    S1W3 = {x = -1374.0517578125, y = -1626.1932373046, z = 2.1679482460022}, -- DONE
    S2W3 = {x = -1371.6577148438, y = -1683.7159423828, z = 0.53731191158294}, -- DONE
    S3W3 = {x = -1270.7962646484, y = -1919.3767089844, z = 2.3391745090484} -- DONE
}


------------------------------
-- DEATH MATCH VENICE       --
------------------------------
Config.Location.Count = 28
Config.Location.VeniceBeach = {
    -- NORTH 3 WEST
    N3W3 = {x = -1485.9119873046, y = -1128.9865722656, z = 0.036846715956926}, -- DONE
    N3W2 = {x = -1362.8782958984, y = -1077.9821777344, z = 3.6049129962922}, -- DONE
    N3W1 = {x = -1296.396484375, y = -1073.1215820312, z = 7.1459565162658}, -- DONE
    N3C = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128}, -- DONE

    -- NORTH 2 WEST
    N2W3 = {x = -1517.623046875, y = -1273.4562988282, z = 3.459242105484}, -- DONE
    N2W2 = {x = -1342.1809082032, y = -1233.6260986328, z = 5.9385232925416}, -- DONE
    N2W1 = {x = -1308.7030029296, y = -1220.2585449218, z = 8.9804773330688}, -- DONE
    N2C = {x = -1233.9226074218, y = -1192.242553711, z = 11.257278442382}, -- DONE

    -- NORTH 1 WEST
    N1W3 = {x = -1533.3830566406, y = -1369.7990722656, z = 0.35384374856948}, -- DONE
    N1W2 = {x = -1386.7277832032, y = -1322.235961914, z = 4.150161743164}, -- DONE
    N1W1 = {x = -1277.4610595704, y = -1301.6264648438, z = 4.0185108184814}, -- DONE
    N1C = {x = -1180.1854248046, y = -1246.3952636718, z = 15.306381225586}, -- DONE

    -- CENTER WEST
    CW3 = {x = -1497.9467773438, y = -1484.6145019532, z = 5.6431970596314}, -- DONE
    CW2 = {x = -1383.0390625, y = -1405.2744140625, z = 2.7274069786072}, -- DONE
    CW1 = {x = -1227.3278808594, y = -1408.0838623046, z = 4.1980233192444}, -- DONE
    CC = {x = -1112.1948242188, y = -1340.5877685546, z = 5.028392791748}, -- DONE

    -- SOUTH 1 WEST
    S1W3 = {x = -1374.0517578125, y = -1626.1932373046, z = 2.1679482460022}, -- DONE
    S1W2 = {x = -1310.8116455078, y = -1522.049194336, z = 4.4167251586914}, -- DONE
    S1W1 = {x = -1218.8154296875, y = -1474.640258789, z = 7.87233877182}, -- DONE
    S1C = {x = -1123.3349609375, y = -1445.7370605468, z = 5.0690126419068}, -- DONE

    -- SOUTH 2 WEST
    S2W3 = {x = -1371.6577148438, y = -1683.7159423828, z = 0.53731191158294}, -- DONE
    S2W2 = {x = -1288.047241211, y = -1606.2890625, z = 4.096655368805}, -- DONE
    S2W1 = {x = -1169.4633789062, y = -1554.0236816406, z = 4.3751873970032}, -- DONE
    S2C = {x = -1081.6381835938, y = -1512.6505126954, z = 4.9379544258118}, -- DONE

    -- SOUTH 3 WEST
    S3W3 = {x = -1270.7962646484, y = -1919.3767089844, z = 2.3391745090484}, -- DONE
    S3W2 = {x = -1189.2487792968, y = -1784.4901123046, z = 15.62109375}, -- DONE
    S3W1 = {x = -1089.1895751954, y = -1666.8175048828, z = 11.617361068726}, -- DONE
    S3C = {x = -973.93908691406, y = -1629.5102539062, z = 2.0695433616638} -- DONE
}

Config.Location.AirCount = 28
Config.Location.MidAir = {
    AIR1 = {x = -171.38845825196, y = -844.44146728516, z = 949.88934326172}, -- Done
    AIR2 = {x = -966.74725341796, y = -873.12329101562, z = 1209.9366455078}, -- Done
    AIR3 = {x = -538.06463623046, y = 82.461128234864, z = 634.75512695312}, -- Done
    AIR4 = {x = -966.74725341796, y = -873.12329101562, z = 1209.9366455078}, -- Done
    AIR5 = {x = -1088.7420654296, y = -882.79608154296, z = 955.98266601562}, -- Done
    AIR6 = {x = -1091.0366210938, y = -876.01550292968, z = 891.66094970704}, -- Done
    AIR7 = {x = -945.14569091796, y = -939.29577636718, z = 722.42193603516}, -- Done
    AIR8 = {x = -1066.4953613282, y = -882.7719116211, z = 494.6128540039}, -- Done
    AIR9 = {x = -1692.4022216796, y = -1068.1123046875, z = 1517.0888671875}, -- Done
    AIR10 = {x = -1408.3129882812, y = -1065.1821289062, z = 958.95379638672}, -- Done
    AIR11 = {x = -1270.2337646484, y = -1536.7869873046, z = 705.96697998046}, -- Done
    AIR12 = {x = -1316.5837402344, y = -1582.4014892578, z = 558.0853881836}, -- Done
    AIR13 = {x = -1252.374633789, y = -1310.0692138672, z = 464.71365356446}, -- Done
    AIR14 = {x = -690.22662353516, y = -466.0432434082, z = 977.06341552734}, -- Done
    AIR15 = {x = -821.86791992188, y = -309.21719360352, z = 914.15856933594}, -- Done
    AIR16 = {x = -888.05041503906, y = 7.0877676010132, z = 756.70556640625}, -- Done
    AIR17 = {x = 1267.3076171875, y = -772.72821044922, z = 1826.3762207032}, -- Done
    AIR18 = {x = 1226.7517089844, y = -673.16082763672, z = 1701.3422851562}, -- Done
    AIR19 = {x = 1136.5573730468, y = -633.83435058594, z = 1662.490600586}, -- Done
    AIR20 = {x = 1034.9936523438, y = -746.40728759766, z = 1614.6405029296}, -- Done
    AIR21 = {x = 926.13818359375, y = -882.94940185546, z = 1554.8880615234}, -- Done
    AIR22 = {x = 736.48229980468, y = -1120.1282958984, z = 1460.3033447266}, -- Done
    AIR23 = {x = 613.27478027344, y = -1272.2885742188, z = 1395.0513916016}, -- Done
    AIR24 = {x = 419.8493347168, y = -1514.4040527344, z = 1292.7465820312}, -- Done
    AIR25 = {x = 154.80763244628, y = -1843.9532470704, z = 1153.258178711}, -- Done
    AIR26 = {x = -357.9055480957, y = -2350.9348144532, z = 898.97668457032}, -- Done
    AIR27 = {x = -620.3452758789, y = -2624.7099609375, z = 782.70727539062}, -- Done
    AIR28 = {x = -620.3452758789, y = -2624.7099609375, z = 982.70727539062} -- Done
}




--[==[
    N3E1 = {x = -1233.9226074218, y = -1192.242553711, z = 11.257278442382},
    N3E2 = {x = -1308.7030029296, y = -1220.2585449218, z = 8.9804773330688},
    N3E3 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},

    N2E1 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},
    N2E2 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},
    N2E3 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},

    N1E1 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},
    N1E2 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},
    N1E3 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},

    CE1 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},
    CE2 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},
    CE3 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},

    S1E1 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},
    S1E2 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},
    S1E3 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},

    S2E1 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},
    S2E2 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},
    S2E3 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},

    S3E1 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},
    S3E2 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128},
    S3E3 = {x = -1263.6274414062, y = -1055.2677001954, z = 8.4164161682128}
}
--]==]

--ref: resources/bob74_ipl/dlc_high_life/apartment1.lua -> apartment6.lua
Config.Location.ApartmentCnt = 6
Config.Location.Apartment = {
    apt1 = {x = -1462.28100000, y = -539.62760000, z = 72.44434000}, -- closest venice beach view
    apt2 = {x = -914.90260000, y = -374.87310000, z = 112.6748},
    apt3 = {x = -609.56690000, y = 51.28212000, z = 96.60023000},
    apt4 = {x = -778.50610000, y = 331.31600000, z = 210.39720},
    apt5 = {x = -22.61353000, y = -590.14320000, z = 78.430910}, -- out in the desert somewhere
    apt6 = {x = -609.56690000, y = 51.28212000, z = -183.98080} -- no view (underground)
}

----------------------------------
-- LOCATION TERRITORIES         --
----------------------------------
Config.Location.Territories = {
    ["terr_tankwars"] = {
        areas = {
          [1] = {
            -- location  = vector3(-147.9323, -1600.784, 38.29156), -- center
            -- width     = 200.0, -- east | west
            -- height    = 280.0, -- north | south
            location  = vector3(-56.761848449708, -1468.6564941406, 32.108127593994), -- center
            width     = 200.0, -- east | west
            height    = 480.0, -- north | south
            heading   = 50,
            display   = 10
            }
        },
        --[==[
        blipData = {
          pos = vector3(-161.6598, -1638.28, 37.2459),
          sprite = 499,
          color = 3,
          text = _U["methlab_blip"],
          display = 3,
          shortRange = true,
          scale = 1.0,
        }
        --]==]
    }
}

-- The color for the blip when gang/job is controlling zone.
Config.Location.BlipColors = {
  police  = 0,
  grove   = 2,
  ballas  = 7,
  vagos   = 70,
  MC  = 1,
}


----------------------------------
-- MAPPED ZONES                 --
----------------------------------
Config.Location.Zones = {
    ["zone_test"] = {
        areas = {
          [1] = {
            -- location  = vector3(-147.9323, -1600.784, 38.29156), -- center
            -- width     = 200.0, -- east | west
            -- height    = 280.0, -- north | south
            location  = vector3(-56.761848449708, -1468.6564941406, 32.108127593994), -- center
            width     = 200.0, -- east | west
            height    = 480.0, -- north | south
            heading   = 50,
            display   = 10
            }
        }
    }
}

Config.Zone.Areas = {

    --[==[
    --set user input params
    local lc    = (location or {})
    local wd    = (tonumber(width) or 50.0) -- float input (east <-> west)
    local ht    = (tonumber(height) or 50.0) -- float input (north <-> sourth)
    local head  = (tonumber(heading) or 50) -- int input (0 -> 360; degrees; i think)
    local disp  = (tonumber(display) or 10) -- int input (off <= 1; on >= 4; on = 2, off = 3)
    local col   = (tonumber(color) or 2) -- int input (0 -> 85; white -> black)
    local alp   = (tonumber(alpha) or 50) -- int input (0 -> 1000+; transparent -> opaque)
    --]==]
    
    --[==[
        --TANKWARS IN-GAME MAPPING
        [1602315303 cmd: /gps][1] _ ENTER
        [1602315303 cmd: /gps][1] DISPLAY _ raw: gps paint 300.0 850.0 110 10 0 100
        [1602315303 cmd: /gps][1] DISPLAY _ args cmd: DISPLAY _ input cmd: '/gps paint 300.0 850.0 110 10 0 100'
        [1602315303 cmd: /gps][1] DISPLAY _ args: (paint, 300.0, 850.0, 110, 10, 0, 100) => (argv1, argv2, argv3, argv4, argv5, argv6, argv7)
        [1602315303 cmd: /gps][1] CURRENT GPS: {x = -1354.1064453125, y = -1457.1844482422, z = 4.2751145362854}
        detected argv1 == 'paint'...
        
        [1602376912 cmd: /gps][1] DISPLAY _ raw: gps paint 300.0 1100.0 120 10 7 100
        [1602376912 cmd: /gps][1] DISPLAY _ args cmd: DISPLAY _ input cmd: '/gps paint 300.0 1100.0 120 10 7 100'
        [1602376912 cmd: /gps][1] DISPLAY _ args: (paint, 300.0, 1100.0, 120, 10, 0, 100) => (argv1, argv2, argv3, argv4, argv5, argv6, argv7)
        [1602376912 cmd: /gps][1] CURRENT GPS: {x = -1386.7302246094, y = -1322.2459716796, z = 4.1496329307556}
        detected argv1 == 'paint'...
    --]==]
    tankwars = {
        area  = {
            location  = vector3(-1386.7302246094, -1322.2459716796, 4.1496329307556), -- center
            width     = 300.0, -- east | west (float)
            height    = 1100.0, -- north | south (float)
            heading   = 120, -- (int)
            display   = 2, -- (int)
            color     = 0, -- color of blip area (int)
            alpha     = 100 -- color alpha value (int)
        }
    },
    
    
    --[==[
        --WAVEWARS IN-GAME MAPPING
        [1602375886 cmd: /gps][1] _ ENTER
        [1602375886 cmd: /gps][1] DISPLAY _ raw: gps paint 500.0 1100.0 120 10 7 100
        [1602375886 cmd: /gps][1] DISPLAY _ args cmd: DISPLAY _ input cmd: '/gps paint 500.0 1100.0 120 10 7 100'
        [1602375886 cmd: /gps][1] DISPLAY _ args: (paint, 500.0, 1100.0, 120, 10, 7, 100) => (argv1, argv2, argv3, argv4, argv5, argv6, argv7)
        [1602375886 cmd: /gps][1] CURRENT GPS: {x = -1654.4947509766, y = -1618.3524169922, z = 0.5876750946045}
        detected argv1 == 'paint'...
    --]==]
    wavewars = {
        area  = {
            location  = vector3(-1654.4947509766, -1618.3524169922, 0.5876750946045), -- center
            width     = 500.0, -- east | west (float)
            height    = 1100.0, -- north | south (float)
            heading   = 120, -- (int)
            --heading   = 115, -- (int)
            display   = 2, -- (int)
            color     = 2, -- color of blip area (int)
            alpha     = 100 -- color alpha value (int)
        }
    }
}
