namespace DUE_FSharp_SPASandbox_2026

open System
open WebSharper
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.Sitelets

type EndPoint =
    | [<EndPoint "">] Home
    | [<EndPoint "/planner">] Planner
    | [<EndPoint "/about">] About

[<JavaScript>]
module Client =

    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    type StudyTask =
        {
            Id: int
            Text: string
            Deadline: string
            IsCompleted: bool
        }

    let tasksVar =
        Var.Create [
            { Id = 1; Text = "Matematika beadandó"; Deadline = "2026-04-28"; IsCompleted = false }
            { Id = 2; Text = "Hálózatok jegyzet átnézése"; Deadline = "2026-04-30"; IsCompleted = false }
            { Id = 3; Text = "F# gyakorló feladat"; Deadline = "2026-05-03"; IsCompleted = true }
        ]

    let nextTaskId = Var.Create 4

    let router = Router.Infer<EndPoint>()
    let currentPage = Router.InstallHash EndPoint.Home router

    let parseDeadline (deadline: string) =
        if String.IsNullOrWhiteSpace(deadline) then None
        else
            try
                Some(DateTime.Parse(deadline))
            with
            | _ -> None

    let daysLeftText (deadline: string) =
        match parseDeadline deadline with
        | Some date ->
            let days = (date.Date - DateTime.Today).Days
            if days < 0 then
                string (-days) + " days overdue"
            elif days = 0 then
                "Due today"
            elif days = 1 then
                "1 day left"
            else
                string days + " days left"
        | None ->
            "No deadline"

    let activeTasks tasks =
        tasks |> List.filter (fun task -> not task.IsCompleted)

    let completedTasks tasks =
        tasks |> List.filter (fun task -> task.IsCompleted)

    let urgentTasks tasks =
        tasks
        |> activeTasks
        |> List.sortBy (fun task ->
            match parseDeadline task.Deadline with
            | Some date -> date
            | None -> DateTime.MaxValue
        )
        |> List.truncate 3

    let addTask (taskText: string) (deadline: string) =
        let trimmedText = taskText.Trim()
        let trimmedDeadline = deadline.Trim()

        if trimmedText <> "" && trimmedDeadline <> "" then
            let newTask =
                {
                    Id = nextTaskId.Value
                    Text = trimmedText
                    Deadline = trimmedDeadline
                    IsCompleted = false
                }

            tasksVar.Value <- tasksVar.Value @ [ newTask ]
            nextTaskId.Value <- nextTaskId.Value + 1

    let removeTask (taskId: int) =
        tasksVar.Value <-
            tasksVar.Value
            |> List.filter (fun task -> task.Id <> taskId)

    let toggleTask (taskId: int) =
        tasksVar.Value <-
            tasksVar.Value
            |> List.map (fun task ->
                if task.Id = taskId then
                    { task with IsCompleted = not task.IsCompleted }
                else
                    task
            )

    module Pages =

        let Home () =
            div [attr.``class`` "space-y-6"] [
                div [attr.``class`` "bg-white rounded-xl shadow-md p-8 space-y-3"] [
                    h1 [attr.``class`` "text-3xl font-bold text-gray-800"] [
                        text "Smart Study Planner"
                    ]
                    p [attr.``class`` "text-gray-600"] [
                        text "A simple planner for tracking study tasks and upcoming deadlines."
                    ]
                ]

                tasksVar.View
                |> View.Map (fun tasks ->
                    let activeCount = activeTasks tasks |> List.length
                    let completedCount = completedTasks tasks |> List.length
                    let totalCount = List.length tasks
                    let topUrgent = urgentTasks tasks

                    div [attr.``class`` "space-y-6"] [
                        div [attr.``class`` "grid md:grid-cols-3 gap-4"] [
                            div [attr.``class`` "bg-blue-50 rounded-lg p-4 border border-blue-100"] [
                                h2 [attr.``class`` "text-lg font-semibold text-gray-800"] [
                                    text "Total tasks"
                                ]
                                p [attr.``class`` "text-3xl font-bold text-blue-700 mt-2"] [
                                    text (string totalCount)
                                ]
                            ]

                            div [attr.``class`` "bg-yellow-50 rounded-lg p-4 border border-yellow-100"] [
                                h2 [attr.``class`` "text-lg font-semibold text-gray-800"] [
                                    text "Active tasks"
                                ]
                                p [attr.``class`` "text-3xl font-bold text-yellow-700 mt-2"] [
                                    text (string activeCount)
                                ]
                            ]

                            div [attr.``class`` "bg-green-50 rounded-lg p-4 border border-green-100"] [
                                h2 [attr.``class`` "text-lg font-semibold text-gray-800"] [
                                    text "Completed tasks"
                                ]
                                p [attr.``class`` "text-3xl font-bold text-green-700 mt-2"] [
                                    text (string completedCount)
                                ]
                            ]
                        ]

                        div [attr.``class`` "bg-white rounded-xl shadow-md p-6 space-y-4"] [
                            h2 [attr.``class`` "text-2xl font-semibold text-gray-800"] [
                                text "Most urgent tasks"
                            ]

                            if List.isEmpty topUrgent then
                                div [attr.``class`` "text-gray-500"] [
                                    text "No active tasks with deadline."
                                ]
                            else
                                yield!
                                    topUrgent
                                    |> List.map (fun task ->
                                        div [attr.``class`` "border border-gray-200 rounded-lg p-4 flex flex-col md:flex-row md:items-center md:justify-between gap-2"] [
                                            div [attr.``class`` "space-y-1"] [
                                                p [attr.``class`` "font-semibold text-gray-800"] [
                                                    text task.Text
                                                ]
                                                p [attr.``class`` "text-sm text-gray-500"] [
                                                    text ("Deadline: " + task.Deadline)
                                                ]
                                            ]

                                            span [attr.``class`` "text-sm font-medium text-red-600"] [
                                                text (daysLeftText task.Deadline)
                                            ]
                                        ]
                                    )
                        ]
                    ]
                )
                |> Doc.EmbedView
            ]

        let Planner () =
            let newTask = Var.Create ""
            let newDeadline = Var.Create ""

            div [attr.``class`` "bg-white rounded-xl shadow-md p-8 space-y-6"] [
                div [attr.``class`` "space-y-3"] [
                    h1 [attr.``class`` "text-3xl font-bold text-gray-800"] [
                        text "Planner"
                    ]
                    p [attr.``class`` "text-gray-600"] [
                        text "Add your study tasks and set a deadline for each one."
                    ]
                ]

                div [attr.``class`` "bg-gray-50 rounded-lg p-4 border border-gray-200 space-y-4"] [
                    h2 [attr.``class`` "text-xl font-semibold text-gray-800"] [
                        text "Add new study task"
                    ]

                    div [attr.``class`` "grid md:grid-cols-3 gap-3"] [
                        Doc.InputType.Text [
                            attr.``class`` "px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-400"
                            attr.placeholder "Enter a study task"
                        ] newTask

                        Doc.InputType.Date [
                            attr.``class`` "px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-400"
                        ] newDeadline

                        button [
                            attr.``class`` "bg-blue-600 hover:bg-blue-700 text-white font-semibold px-5 py-2 rounded-lg"
                            on.click (fun _ _ ->
                                addTask newTask.Value newDeadline.Value
                                newTask.Value <- ""
                                newDeadline.Value <- ""
                            )
                        ] [
                            text "Add task"
                        ]
                    ]
                ]

                div [attr.``class`` "space-y-3"] [
                    h2 [attr.``class`` "text-xl font-semibold text-gray-800"] [
                        text "Study task list"
                    ]

                    tasksVar.View
                    |> View.Map (fun tasks ->
                        tasks
                        |> List.sortBy (fun task ->
                            match parseDeadline task.Deadline with
                            | Some date -> date
                            | None -> DateTime.MaxValue
                        )
                        |> List.map (fun task ->
                            let cardClass =
                                if task.IsCompleted then
                                    "bg-gray-100 border border-gray-200 rounded-lg px-4 py-3 shadow-sm flex flex-col md:flex-row md:items-center md:justify-between gap-3 opacity-70"
                                else
                                    "bg-white border border-gray-200 rounded-lg px-4 py-3 shadow-sm flex flex-col md:flex-row md:items-center md:justify-between gap-3"

                            let textClass =
                                if task.IsCompleted then
                                    "text-gray-500 line-through"
                                else
                                    "text-gray-800"

                            div [attr.``class`` cardClass] [
                                div [attr.``class`` "flex items-start gap-3"] [
                                    input (
                                        [
                                            attr.``type`` "checkbox"
                                            on.change (fun _ _ ->
                                                toggleTask task.Id
                                            )
                                        ]
                                        @ (if task.IsCompleted then [attr.``checked`` "checked"] else [])
                                    ) []

                                    div [attr.``class`` "space-y-1"] [
                                        p [attr.``class`` ("font-semibold " + textClass)] [
                                            text task.Text
                                        ]
                                        p [attr.``class`` "text-sm text-gray-500"] [
                                            text ("Deadline: " + task.Deadline)
                                        ]
                                        p [attr.``class`` "text-sm text-blue-600"] [
                                            text (daysLeftText task.Deadline)
                                        ]
                                    ]
                                ]

                                button [
                                    attr.``class`` "bg-red-500 hover:bg-red-600 text-white font-semibold px-4 py-2 rounded-lg"
                                    on.click (fun _ _ ->
                                        removeTask task.Id
                                    )
                                ] [
                                    text "Delete"
                                ]
                            ]
                        )
                        |> Doc.Concat
                    )
                    |> Doc.EmbedView
                ]
            ]

        let About () =
            div [attr.``class`` "bg-white rounded-xl shadow-md p-8 space-y-4"] [
                h1 [attr.``class`` "text-3xl font-bold text-gray-800"] [
                    text "About"
                ]
                p [attr.``class`` "text-gray-600"] [
                    text "Smart Study Planner is a simple single page application for managing study tasks and deadlines."
                ]
                p [attr.``class`` "text-gray-600"] [
                    text "The project was created with F#, WebSharper and Tailwind CSS."
                ]
                p [attr.``class`` "text-gray-600"] [
                    text "Users can add tasks, set deadlines, mark tasks as completed and remove tasks from the list."
                ]
            ]

    [<SPAEntryPoint>]
    let Main () =

        let renderInnerPage (currentPage: Var<EndPoint>) =
            currentPage.View.Map(fun endpoint ->
                match endpoint with
                | Home -> Pages.Home()
                | Planner -> Pages.Planner()
                | About -> Pages.About()
            )
            |> Doc.EmbedView

        IndexTemplate()
            .Content(renderInnerPage currentPage)
            .Bind()