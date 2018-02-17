var roomMockData = (function () {

    return {
        getAllRooms: getAllRooms,
        getSingleMockRoom: getAllRooms()[0]
    };

    function getAllRooms() {
        return [{
            "id": 1,
            "roomName": "Room1",
            "computerCount": 8,
            "phoneCount": 1,
            "seatCount": 8,
            "smartRoom": false,
            "active": true,
            "location": {
                "id": 1,
                "name": "LocationOne",
                "rooms": null,
                "locationCredentials": null,
                "active": false,
                "additionalInformation": null
            },
            "rooms": null,
            "bookings": null
        }, {
            "id": 2,
            "roomName": "Room_2",
            "computerCount": 2,
            "phoneCount": 2,
            "seatCount": 6,
            "smartRoom": false,
            "active": true,
            "location": {
                "id": 1,
                "name": "LocationOne",
                "rooms": null,
                "locationCredentials": null,
                "active": false,
                "additionalInformation": null
            },
            "rooms": null,
            "bookings": null
        }]
    }

})();
