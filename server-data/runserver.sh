
# License key: use w/ environment variable (for: sv_licenseKey changeme)
#  add to ~/.bashrc: export CFX_KEY=your_key
#  OR add to /etc/environment (w/o 'export' keyword): CFX_KEY=your_key
# then run: $ .../run.sh +exec server.cfg +set sv_licenseKey $CFX_KEY

# NOTE: place this file in the same dir as the server.cfg
#  DL & extract: https://runtime.fivem.net/artifacts/fivem/build_proot_linux/master/ 
#   into: /srv/dm4c-fivem/_server
# then execute this file (in the same dir as the server.cfg): $ runserver.sh
/srv/dm4c-fivem/_server/run.sh +exec server.cfg +set sv_licenseKey $CFX_KEY

# supress "hitch warning" (due to yarn issue, when running local vm ubuntu)
# /srv/dm4c-fivem/_server/run.sh +exec server.cfg +set sv_licenseKey $CFX_KEY | grep -v "hitch warning"