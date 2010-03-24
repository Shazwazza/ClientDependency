setTimeout(function() {
    alert("I'm a rogue script");
    setTimeout(function() {
        alert("I'm becoming an annoying script");
    }
    , 1000);
}
, 2000);