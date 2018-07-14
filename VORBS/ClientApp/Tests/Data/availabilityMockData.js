var availabilityMockData = (function () {

    return {
        getAllRoomsForLocation: getAllRoomsForLocation,
        getAvailableRooms: getAvailableRooms
    };

    function getAllRoomsForLocation() {
        return [{
            "id": 1,
            "roomName": "Room_1",
            "computerCount": 2,
            "phoneCount": 1,
            "seatCount": 4,
            "smartRoom": false,
            "active": false,
            "location": null,
            "rooms": null,
            "bookings": [{
                "id": 1,
                "pid": null,
                "owner": "Current User",
                "subject": null,
                "room": null,
                "location": null,
                "numberOfAttendees": 0,
                "externalAttendees": null,
                "startDate": "2017-08-09T09:00:00",
                "endDate": "2017-08-09T11:00:00",
                "flipchart": false,
                "projector": false,
                "dssAssist": false,
                "isSmartMeeting": false,
                "recurrenceId": null
            }, {
                "id": 2,
                "pid": null,
                "owner": "Current User 2",
                "subject": null,
                "room": null,
                "location": null,
                "numberOfAttendees": 0,
                "externalAttendees": null,
                "startDate": "2017-08-10T13:00:00",
                "endDate": "2017-08-10T14:30:00",
                "flipchart": false,
                "projector": false,
                "dssAssist": false,
                "isSmartMeeting": false,
                "recurrenceId": null
            }]
        }, {
            "id": 2,
            "roomName": "Room_2",
            "computerCount": 1,
            "phoneCount": 1,
            "seatCount": 8,
            "smartRoom": false,
            "active": false,
            "location": null,
            "rooms": null,
            "bookings": [{
                "id": 1,
                "pid": null,
                "owner": "Current User",
                "subject": null,
                "room": null,
                "location": null,
                "numberOfAttendees": 0,
                "externalAttendees": null,
                "startDate": "2017-08-07T10:00:00",
                "endDate": "2017-08-07T11:00:00",
                "flipchart": false,
                "projector": false,
                "dssAssist": false,
                "isSmartMeeting": false,
                "recurrenceId": null
            }, {
                "id": 6,
                "pid": null,
                "owner": "Current User 2",
                "subject": null,
                "room": null,
                "location": null,
                "numberOfAttendees": 0,
                "externalAttendees": null,
                "startDate": "2017-08-09T14:00:00",
                "endDate": "2017-08-09T15:30:00",
                "flipchart": false,
                "projector": false,
                "dssAssist": false,
                "isSmartMeeting": false,
                "recurrenceId": null
            }]
        }]
    }

    function getAvailableRooms() {
        return [{
            "id": 2,
            "roomName": "Room_2",
            "computerCount": 1,
            "phoneCount": 1,
            "seatCount": 8,
            "smartRoom": false,
            "active": false,
            "location": null,
            "rooms": null,
            "bookings": [{
                "id": 6,
                "pid": null,
                "owner": "Current User 2",
                "subject": null,
                "room": null,
                "location": null,
                "numberOfAttendees": 0,
                "externalAttendees": null,
                "startDate": "2017-08-09T14:00:00",
                "endDate": "2017-08-09T15:30:00",
                "flipchart": false,
                "projector": false,
                "dssAssist": false,
                "isSmartMeeting": false,
                "recurrenceId": null
            }]
        }];
    }



})();