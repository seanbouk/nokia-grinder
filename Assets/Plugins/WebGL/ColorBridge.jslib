mergeInto(LibraryManager.library, {
  UpdateWebColors: function(background, foreground) {
    var bg = UTF8ToString(background);
    var fg = UTF8ToString(foreground);
    updateColors(bg, fg);
  },

  InitializeWebInput: function() {
    if (typeof window.webInputInitialized === 'undefined') {
      window.webInputInitialized = true;
      window.isWPressed = false;
      
      // Function to be called from the web side
      window.setWKeyState = function(isPressed) {
        window.isWPressed = isPressed;
      };
    }
  },

  IsWPressed: function() {
    return window.isWPressed === true;
  }
});
