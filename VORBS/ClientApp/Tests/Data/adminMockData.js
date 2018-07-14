var adminMockData = (function () {

    return {
        getAdminUser: getAdminUser
    };

    function getAdminUser() {
        return {
            "id": 1,
            "pid": "0000001",
            "firstName": "Dean",
            "lastName": "Martin",
            "location": {
                "id": 1,
                "name": "Location1",
                "rooms": null,
                "locationCredentials": null,
                "active": true,
                "additionalInformation": null
            },
            "email": "fakeemail1@mail.com",
            "permissionLevel": 2
        }
    }

})();