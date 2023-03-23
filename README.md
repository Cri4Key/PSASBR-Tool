PlayStation All-Stars Battle Royale Tool V2
=======
Branch of the development of the tool, trying to make it better. Currently HALTED.

The Good
=======
- This time I know what OOP is, e.g. separating the XANI bundles from the form as an object
- Proof of concept of an updater
- Using threads so it doesn't look stuck while performing operations
- Integration of Scarlet as a library directly, implementing a texture viewer inside the tool
- Implemented model detection to be opened with Noesis

The Bad
=======
- New UI that is probably better than last one, but it stays I am no UI designer so it's still not that good
- The way texture previews are handled is not optimal, having to use a temp folder for them. Need to switch to a direct preview without relying on storage.
- Model detection works, but actually opening them with Noesis does not, but it brings to the right folder at least...
