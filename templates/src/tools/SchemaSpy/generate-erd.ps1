# Java 8 is required to run this script
# https://community.chocolatey.org/packages/jdk8

$server = "localhost";
$db = "Accelist.WebApiStandard";
$type = "pgsql"
# $type = "mssql08"
$folder = "/SchemaSpy/Accelist.WebApiStandard"
$driver = "./postgresql-42.5.0.jar";
# $driver = "./mssql-jdbc-11.2.1.jre8.jar";
$username = "postgres"
$password = "HelloWorld!"

java -jar ./schemaspy-6.1.0.jar `
    -t $type `
    -dp $driver `
    -host $server `
    -db $db `
    -connprops "Encrypt\=False" `
    -u $username `
    -p $password `
    -o $folder `
    -vizjs
