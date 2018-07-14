(function () {

    angular.module('vorbs.services')
        .factory('AvailabilityService', AvailabilityService);

    AvailabilityService.$inject = ['$resource'];

    function AvailabilityService($resource) {
        return $resource('/api/availability/:location/:start/:smartRoom/:end/:numberOfPeople/:existingBookingId',
            {
                location: '@location', start: '@start', smartRoom: '@smartRoom', end: '@end',
                numberOfPeople: '@numberOfPeople', existingBookingId: '@existingBookingId'
            }, {
                get: {
                    method: 'GET',
                    isArray: true
                }
            });
    }

})();