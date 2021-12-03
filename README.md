# 5AAR
 Project for the Advanced User Interfaces course at polimi

## Functions

### Settings Scene

1. destination setting

### FindBusStation Scene

1. find near tabacchi shop
2. find near bus station
3. return the route to the station/shop with steps
4. show and update step information while the user is walking [we set it 10m accuracy](https://docs.unity3d.com/ScriptReference/LocationService.Start.html)
5. add arrow toward the temp destination
6. remind the arrival time of the bus

#### TODO: 

- [ ] image recognition of ticket
- [ ] remind the user to get off the bus by gps location
- [ ] seperate the scene?

 ## Config file

create a config file under ```Asset``` folder in the format below

```xml
<keys>
    <GoogleMapAPIKey>YOUR-GOOGLE-MAP-API-KEY</GoogleMapAPIKey>
</keys>

```

