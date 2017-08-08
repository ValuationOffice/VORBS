(function () {

    angular.module('vorbs.services')
        .factory('AvailabilityService', AvailabilityService);

    AvailabilityService.$inject = ['$resource'];

    function AvailabilityService($resource) {
        return $resource('/api/availability/:location/:start/:smartRoom', { location: '@location', start: '@start', smartRoom: '@smartRoom' }, {
            allRoomBookingsForLocation: {
                method: 'GET',
                isArray: true
            },
            roomsForLocation: {
                method: 'GET',
                isArray: true,
                params: {
                    end: '@end',
                    numberOfPeople: '@numberOfPeople',
                    existingBookingId: '@existingBookingId' 
                },
                url: '/api/availability/:location/:start/:end/:numberOfPeople/:smartRoom/:existingBookingId'
            }
        });
    }

})();