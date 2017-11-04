
var locationMockData = (function () {
    return {
        getMockLocations: getMockLocations,
        getSingleMockLocation: getMockLocations()[0]
    };

    function getMockLocations() {
        return [{
            "id": 1,
            "name": "Location1",
            "rooms": [{
                "id": 1,
                "roomName": "Room1",
                "computerCount": 8,
                "phoneCount": 1,
                "seatCount": 8,
                "smartRoom": false,
                "active": true,
                "location": null,
                "rooms": null,
                "bookings": null
            },
            {
                "id": 2,
                "roomName": "Room_2",
                "computerCount": 2,
                "phoneCount": 2,
                "seatCount": 6,
                "smartRoom": false,
                "active": true,
                "location": null,
                "rooms": null,
                "bookings": null
            }],
            "locationCredentials": [],
            "active": true,
            "additionalInformation": null
        }, {
            "id": 2,
            "name": "Location2",
            "rooms": [{
                "id": 1,
                "roomName": "POD 1",
                "computerCount": 1,
                "phoneCount": 1,
                "seatCount": 5,
                "smartRoom": false,
                "active": true,
                "location": null,
                "rooms": null,
                "bookings": null
            },
            {
                "id": 2,
                "roomName": "POD 2",
                "computerCount": 0,
                "phoneCount": 1,
                "seatCount": 3,
                "smartRoom": false,
                "active": true,
                "location": null,
                "rooms": null,
                "bookings": null
            }],
            "locationCredentials": [],
            "active": true,
            "additionalInformation": null
        }];
    }
})();

