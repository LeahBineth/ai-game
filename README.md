# ai-game
Tic Tac Toe game playing against Open AI

Every time you call the open ai API, the model has no memory of the previous requests you have made. In other words: 		each API call is a standalone interaction. if you want the api models to remember the previous messages you have to send the entire conversation back to the API each time.

Tokens: Before the API processes the prompts, the input is broken down into tokens. Tokens can be thought of as pieces of words.
  
#### Role management in open ai api:
	1. System: Provides high-level instructions. Allows you to specify the way the model answers questions.
	2. User: Prompts made by the user.
	3. Assistant: Model’s responses (based on the users prompts)

#### Examples of prompt properties:
	1. Messages: prompts made by user.
	2. Temperature: this affects the creativity of the response. (It passes in randomness into the responses. the value can be 0 through 1.0, where 0 is the least random and 1.0 is the most.)
	3. MaxTokens: sets how large your request and response can be.
	4. NumChoicesPerPrompt: sets number of responses user wants to get per request. 

#### Prompt engineering: (tips)
	1. Include details in your query to get more relevant answers.
	2. Use delimiters to clearly indicate distinct parts of the input.
	3. Specify the steps required to complete a task.
		 Some tasks are best specified as a sequence of steps. Writing the steps out explicitly can make it easier for the model to follow them.
	4. Provide examples. Two ways of setting up your prompt:
	    a. Zero-shot learning: The user sets up a message or prompt without providing specific examples related to the task. The model relies on its general pre-trained knowledge to generate a response or perform  the task.
	    b. Few-shot learning: In this setting, the user includes a small amount of specific examples or additional information in the message or prompt. This assists the model by giving it a bit of targeted data to improve its understanding or performance on the task.     
	5. Specify the desired length of the output. e.g. Summarize the text delimited by triple quotes in about 50 words.
