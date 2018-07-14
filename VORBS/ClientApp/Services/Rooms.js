(function () {

    angular.module('vorbs.services')
        .factory('RoomsService', RoomsService);

    RoomsService.$inject = ['$resource'];

    function RoomsService($resource) {
        return $resource('/api/room', {}, {
            query: {
                method: 'GET',
                isArray: true
            },
            getByID: {
                method: 'GET',
                params: {
                    id: '@id'
                },
                url: '/api/room/:id'
            },
            getByStatus: {
                method: 'GET',
                isArray: true,
                params: {
                    locationName: '@locationName',
                    status: '@status'
                },
                url: '/api/room/:locationName/:status'
            },
            getByName: {
                method: 'GET',
                params: {
                    locationId: '@locationId',
                    roomName: '@roomName'
                }, 
                url: '/api/room/:locationId/:roomName'
            },
            update: {
                method: 'PUT',
                params: {
                    id: '@id'
                },
                url: '/api/room/:id'
            },
            updateStatus: {
                method: 'PATCH',
                params: {
                    id: '@id',
                    status: '@status'
                },
                url: '/api/room/:id/:status'
            }
        });
    }

})();