(function () {

    angular.module('vorbs.services')
        .factory('LocationsService', LocationsService);

    LocationsService.$inject = ['$resource'];

    function LocationsService($resource) {

        function sortLocationByName(locations) {

            var locationArray = locations || [];

            if (locationArray.length > 0) {
                locationArray = locationArray.sort(function (a, b) {
                    return (a.name > b.name) ? 1 : ((b.name > a.name) ? -1 : 0);
                });
            }

            return locationArray;
        }

        return $resource('/api/locations', {},
            {
                query: {
                    method: 'GET',
                    isArray: true,
                    interceptor: {
                        response: function (resp) {

                            var data = resp.data;

                            if (data) {
                                data = sortLocationByName(data);
                            }

                            return data;
                        }
                    }
                },
                getByID: {
                    method: 'GET',
                    params: {
                        id: '@id'
                    },
                    url: '/api/locations/:id'
                },
                getByStatus: {
                    method: 'GET',
                    isArray: true,
                    params: {
                        status: '@status',
                        extraInfo: '@extraInfo'
                    },
                    interceptor: {
                        response: function (resp) {

                            var data = resp.data;

                            if (data) {
                                data = sortLocationByName(data);
                            }

                            return data;
                        }
                    },
                    url: '/api/locations/:status/:extraInfo'
                },
                create: {
                    method: 'POST'
                },
                update: {
                    method: 'PUT',
                    params: {
                        id: '@id'
                    },
                    url: '/api/locations/:id'
                },
                updateStatus: {
                    method: 'PATCH',
                    params: {
                        id: '@id',
                        status: '@status'
                    },
                    url: '/api/locations/:id/:status'
                }
            });
    }

})();