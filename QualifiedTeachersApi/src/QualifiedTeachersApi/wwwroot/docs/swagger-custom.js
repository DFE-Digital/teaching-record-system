window.fetch = function (fetch) {
  return function () {
    var req = arguments[1];
    if (req.headers["X-Requested-With"]) {
      delete req.headers["X-Requested-With"];
    }
    return fetch.apply(window, arguments);
  };
}(window.fetch);
