(function () {

    angular.module('vorbs.services')
        .factory('UsersService', UsersService);

    UsersService.$inject = ['$resource'];

    function UsersService($resource) {

        return $resource('/api/users/:name', { name: '@name' },
            {
                get: {
                    method: 'GET',
                    isArray: true
                }
            });
    }
})();