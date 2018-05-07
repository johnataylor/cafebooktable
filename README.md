# cafebooktable
BookTable dialog for Cafe bot

- this demonstrates how to write a Waterfall dialog encapsulated in a custom DialogContainer
- it includes Choice, Number and a custom TimexPrompt
- the custom TimexPrompt is similar to the built in DateTimePrompt (but doesn't drop resolutions)
- the TimexPrompt uses a custom Validator that resolves with constraints the TIMEX expression from the recognizer using the TimexProperty library

