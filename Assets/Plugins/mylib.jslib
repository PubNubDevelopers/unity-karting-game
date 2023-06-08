mergeInto(LibraryManager.library, {

    CompleteAction: function (feature) {
       //  DEMO: used by the interactive demo
        actionCompleted({
            action: UTF8ToString(feature),
            debug: false
        })
    }   
});