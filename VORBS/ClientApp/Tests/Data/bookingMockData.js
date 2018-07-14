/* jshint -W079 */
var bookingMockData = (function () {
    return {
        getMockBookings: getMockBooking,
        getSingleMockBooking: getMockBooking()[0]
    };

    function getMockBooking() {
        return [{
            "id": 1,
            "pid": "0000001",
            "owner": "Current User",
            "subject": "",
            "room": {
                "id": 1,
                "roomName": "Room1",
                "computerCount": 1,
                "phoneCount": 1,
                "seatCount": 5,
                "smartRoom": false,
                "active": true,
                "location": null,
                "rooms": null,
                "bookings": null
            },
            "location": {
                "id": 1,
                "name": "Location1",
                "rooms": null,
                "locationCredentials": [],
                "active": false,
                "additionalInformation": null
            },
            "numberOfAttendees": 0,
            "externalAttendees": [{
                "id": 1,
                "bookingID": 13,
                "fullName": "John Doe",
                "companyName": "N/A",
                "passRequired": true,
                "booking": null
            }],
            "startDate": "2017-07-20T13:30:00",
            "endDate": "2017-07-20T14:00:00",
            "flipchart": false,
            "projector": true,
            "dssAssist": false,
            "isSmartMeeting": false,
            "recurrenceId": null
        }, {
            "id": 2,
            "pid": "0000001",
            "owner": "Current User",
            "subject": "",
            "room": {
                "id": 1,
                "roomName": "Room.2",
                "computerCount": 3,
                "phoneCount": 1,
                "seatCount": 8,
                "smartRoom": false,
                "active": true,
                "location": null,
                "rooms": null,
                "bookings": null
            },
            "location": {
                "id": 1,
                "name": "Location2",
                "rooms": null,
                "locationCredentials": [],
                "active": false,
                "additionalInformation": null
            },
            "numberOfAttendees": 0,
            "externalAttendees": [],
            "startDate": "2017-07-21T09:00:00",
            "endDate": "2017-07-21T12:30:00",
            "flipchart": false,
            "projector": false,
            "dssAssist": false,
            "isSmartMeeting": false,
            "recurrenceId": null
        }];
    }

})();
