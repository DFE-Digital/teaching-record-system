ENV_VARS=$(env | grep DefaultConnection | sed 's/^ConnectionStrings__DefaultConnection=//' | awk -v RS=';' -v FS='=' '
/^Server/ { print "export PGHOST=" $2 }
/^Database/ { print "export PGDATABASE=" $2 }
/^User Id/ { print "export PGUSER=" $2 }
/^Password/ { print "export PGPASSWORD=" substr($0, 10) }
')
eval "$ENV_VARS"
psql
