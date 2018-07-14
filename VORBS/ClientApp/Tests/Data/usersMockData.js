var usersMockData = (function () {

    return {
        getMockUsers: getMockUsers,
        getFilteredMockUsers: getFilteredMockUsers
    };

    function getMockUsers() {
        return [{ "pid": "0000001", "firstName": "Paul", "lastName": "McCartney", "email": "user1@email.com" },
        { "pid": "0000002", "firstName": "Ringo", "lastName": "Starr", "email": "user2@email.com" },
        { "pid": "0000003", "firstName": "John", "lastName": "Lennon", "email": "user3@email.com" },
        { "pid": "0000004", "firstName": "George", "lastName": "Harrison", "email": "user4@email.com" }]
    }

    function getFilteredMockUsers() {
        return [].concat(getMockUsers()[0]);
    }

})();

