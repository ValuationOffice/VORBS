(function () {
    angular.module('vorbs.myBookings', [])
        .filter('filterMyBookings', filterMyBookings);

    function filterMyBookings() {
        return function (items, field, reverse) {
            var filteredItems = [];
            angular.forEach(items, function (item) {
                filteredItems.push(item);
            });
            filteredItems.sort(function (a, b) {
                switch (field) {
                    case 'location':
                        return (a.location.name.toLowerCase() > b.location.name.toLowerCase() ? 1 : -1);
                        break;
                    case 'room':
                        return (a.room.roomName.toLowerCase() > b.room.roomName.toLowerCase() ? 1 : -1);
                        break;
                    case 'date':
                        var valueA = new moment(a.startDate);
                        var valueB = new moment(b.startDate);
                        return (valueA > valueB ? 1 : -1);
                        break;
                    case 'startTime':
                        var momentAOrig = new moment(a.startDate);
                        var momentBOrig = new moment(b.startDate);

                        var momentA = new moment();
                        var momentB = new moment();

                        momentA.hour(momentAOrig.hour());
                        momentA.minute(momentAOrig.minute());

                        momentB.hour(momentBOrig.hour());
                        momentB.minute(momentBOrig.minute());

                        return (momentA > momentB ? 1 : -1);

                        break;
                    default:
                        return (a[field] > b[field] ? 1 : -1);
                        break;
                }
            });
            if (reverse) {
                filteredItems.reverse();
            }
            return filteredItems;
        }
    };

})();




