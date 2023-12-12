# OrdersExtractor
Extracts the orders from SMS and add as tasks into Todoist project

 * In order to calibrate all current packages - 
 * You can create a temp project 'test', and sync with him, the app will save all already synced packages,
 * after that, change the project name to the required one, and next sync will sync only the updated


## Additional Data
- The task will be created by default as Priority 3 (Blue), and the assignee will be the owner of the todoist token, additionally, a 'Package' label will be assigned to each order

- When 'Sync Finished' toast appeared - that means that all synced packages saved into SP data, if this toast doesnt appeared, the metadata of already saved packages aren't synced correctly
