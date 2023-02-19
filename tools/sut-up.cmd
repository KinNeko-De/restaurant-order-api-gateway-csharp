:: starts the system under test
docker network create restaurant-dev-net

pushd ..\restaurant.order.apigateway
dotnet publish -o ../sut/publish -c Release --no-self-contained
popd

pushd ..\sut
docker compose --verbose up --build --remove-orphans
popd

pause
