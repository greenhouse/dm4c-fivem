# dm4c-fivem
dm4c c# fivem client/server integration

## test servers
    
## NEXT
    - integrate plainrain
    DONE - start c# and lua scripting for DM4C requirements (needs finalized design)
    N/A - TG: @the_xyt (github: breardon2011)
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



