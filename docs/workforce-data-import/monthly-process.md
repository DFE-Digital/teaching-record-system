# Monthly process to import workforce data in TRS

## Overview

The Teachers Pensions Scheme (TPS) system contains data relating to people with a teaching pension which can be used to create a more accurate picture of the teaching workforce.

An extract is created each month which contains data for approximately 500k “active” teaching records (people with at least one pension contribution activity in the last 6 months).  
These will include anyone entitled to a teaching pension (and includes none teachers).  
We need to load this into TRS each month.

## Steps

### 1. Download files from secure link with TPS

- Check files are available:
  - 2 files should be ready to download on the 25th of each month from an agreed secure location.  
  The files will be named Workforce-Dataset-1-*YYYYMMDD*-1329.csv and Workforce-Dataset-2-*YYYYMMDD-1329.csv where *YYYYMMDD* is the year, month and day of the extract date.  
  - If this is not the case then email the contact at TPS to chase when it will be available.

- Ensure files are in CSV format:
  - The files contain 7 header lines which include the word **RESTRICTED** which need to be removed to truly make the file a **CSV**.  
  - Open each file in a text editor[^1] such as **Notepad** or **Visual Studio Code**.
  - Delete the additional header lines above the field names.
  - Save the file.

### 2. Upload files to Azure blob storage

- Request access to the **s189 TRA production PIM** group at Home -> Privileged Identity Management -> My Roles -> Groups.
- Navigate to the blob storage container where the files should be uploaded at Home -> Storage accounts -> s189p01trspdsa -> Containers -> tps-extracts.
- Upload both files to a virtual folder called **Pending**.
- Delete the files from all other temporary locations other than the **Pending** folder.

### 3. Trigger the import job in Hangfire

- Navigate to the TRS Hangfire dashboard at https://trs-production-ui.teacherservices.cloud/_hangfire (you will need to be set as an Administrator in TRS).
- Click **Recurring Jobs**.
- Click the checkbox next to **ImportTpsCsvExtractFileJob**.
- Click the **Trigger now** button.
- Click on **Jobs** and monitor until there is no longer anything *Processing* (this can take around 20 minutes).
- Repeat above steps in order for the 2nd file to get processed.

### 4. Check the TRS database for any issues with the import

#### Connect to a Kubernetes pod

- Request access to the **s189 TRA production PIM** group at Home -> Privileged Identity Management -> My Roles -> Groups (if not already done previously).
- Start a **Powershell** session in a tool such as **Visual Studio Code** or **Windows Terminal**.
- Login to Azure using Azure CLI:
  - Run `az login` if not logged in previously. This will take you to the Azure portal to authenticate the CLI.
- Install Kubernetes tools **kubectl** and **kubelogin**:
  - Run `az aks install-cli`.
- Set the current Azure account / subscription:
  - Run `az account set --subscription s189-teacher-services-cloud-production`.
- Setup local Kubernetes config:
  - Run `az aks get-credentials --overwrite-existing -g s189p01-tsc-pd-rg -n s189p01-tsc-production-aks`.
  - Run `kubelogin convert-kubeconfig -l azurecli`.
- Start an interactive shell in the Kubernetes pod running the TRS Worker app:
  - Run `kubectl exec --stdin --tty deploy/trs-production-worker -n tra-production -- /bin/ash`.

#### Query the TRS database

- Connect to the TRS Postgres database and start a **psql** session:
  - Run `./db.sh`.
- Get details of the latest 2 imported file for a given extract from the **tps_csv_extract** table where *YYYYMMDD* is the year, month and day of the extract date.
  - Execute the query
    ```
    SELECT
        *
    FROM
        tps_csv_extracts
    WHERE
        filename LIKE '%YYYYMMDD%'
    ORDER BY
        created_on DESC
    LIMIT
        2;
    ```
- Get the counts of records with valid / invalid format fields from the initial import from the CSV files into the **tps_csv_extract_load_items** table where *YYYYMMDD* is the year, month and day of the extract date:
  - Execute the query
    ```
    WITH
        latest_extracts AS (
            SELECT
                *
            FROM
                tps_csv_extracts
            WHERE
                filename LIKE '%YYYYMMDD%'
            ORDER BY
                created_on DESC
            LIMIT
                2
        )
    SELECT
        errors,
        COUNT(1)
    FROM
        tps_csv_extract_load_items x
    WHERE
        EXISTS (
            SELECT
                1
            FROM
                latest_extracts le
            WHERE
                le.tps_csv_extract_id = x.tps_csv_extract_id
        )
    GROUP BY
        errors;
    ```
- Drill into the detail of any records with errors (the `errors` field in the `tps_csv_extract_load_items` table is a flag enum which can contain details of multiple format errors for a given row in the import file) where *YYYYMMDD* is the year, month and day of the extract date:
  - Execute the query  
    ```
    WITH
      latest_extracts AS (
          SELECT
              *
          FROM
              tps_csv_extracts
          WHERE
              filename LIKE '%YYYYMMDD%'
          ORDER BY
              created_on DESC
          LIMIT
              2
      )
    SELECT
        *
    FROM
        tps_csv_extract_load_items x
    WHERE
        EXISTS (
            SELECT
                1
            FROM
                latest_extracts le
            WHERE
                le.tps_csv_extract_id = x.tps_csv_extract_id
        )
        AND errors != 0;
    ```  
- Table of possible formatting errors where the `error_number` is the 0 based bit number representing the error in the `errors` flag

| error_number  | `errors` value (if no other errors) | Field with format error     |
|---------------|-------------------------------------|-----------------------------|
| 0             | 1                                   | TRN                         |
| 1             | 2                                   | National Insurance Number   |
| 2             | 4                                   | Date Of Birth               |
| 3             | 8                                   | Date Of Death               |
| 4             | 16                                  | Member Postcode             |
| 5             | 32                                  | Member Email Address        |
| 6             | 64                                  | Local Authority Code        |
| 7             | 128                                 | Establishment Number        |
| 8             | 256                                 | Establishment Postcode      |
| 9             | 512                                 | Member Id                   |
| 10            | 1024                                | Employment Start Date       |
| 11            | 2048                                | Employment End Date         |
| 12            | 4096                                | Full Or Part Time Indicator |
| 13            | 8192                                | Withdrawl Indicator         |
| 14            | 16384                               | Extract Date                |
| 15            | 32768                               | Gender                      |
| 15            | 65536                               | Establishment Email Address |

- Drill into the detail of any records with specific formatting errors and *YYYYMMDD* is the year, month and day of the extract date:
  - Execute the query
  ```
  WITH
    latest_extracts AS (
        SELECT
            *
        FROM
            tps_csv_extracts
        WHERE
            filename LIKE '%YYYYMMDD%'
        ORDER BY
            created_on DESC
        LIMIT
            2
    )
  SELECT 
      *
  FROM
      tps_csv_extract_load_items x
  WHERE
      EXISTS (
          SELECT
              1
          FROM
              latest_extracts le
          WHERE
              le.tps_csv_extract_id = x.tps_csv_extract_id
      )
      AND (errors >> error_number) & 1 = 1;
  ```
  - Make a note of any fields with incorrect formats in order to feedback to TPS.
  - The most common fields which may have formatting issues are the `National Insurance Number`, `Member Email Address` and `Establishment Email Address`.
  
- Get the counts of valid / invalid records after trying to match to a TRN and Establishment and create or update a `person_employments` record where *YYYYMMDD* is the year, month and day of the extract date:
  - Execute the query
    ```
    WITH
        latest_extracts AS (
            SELECT
                *
            FROM
                tps_csv_extracts
            WHERE
                filename LIKE '%YYYYMMDD%'
            ORDER BY
                created_on DESC
            LIMIT
                2
        )
    SELECT
        result,
        CASE
            WHEN result = 1 THEN 'person_employments record added'
            WHEN result = 2 THEN 'person_employments record updated'
            WHEN result = 3 THEN 'no persons record found in TRS with the given TRN'
            WHEN result = 4 THEN 'no establishments record found in TRS with the given Local Authority Code and Establishment Number'
        END AS description,
        COUNT(1)
    FROM
        tps_csv_extract_items x
    WHERE
        EXISTS(
            SELECT
                1
            FROM
                latest_extracts le
            WHERE
                le.tps_csv_extract_id = x.tps_csv_extract_id
        )
    GROUP BY
        result;
    ```
- Drill into records where no `persons` record could be found which match the TRN provided where *YYYYMMDD* is the year, month and day of the extract date:
  - Execute the query
    ```
    WITH
        latest_extracts AS (
            SELECT
                *
            FROM
                tps_csv_extracts
            WHERE
                filename LIKE '%YYYYMMDD%'
            ORDER BY
                created_on DESC
            LIMIT
                2
        )
    SELECT DISTINCT
        trn
    FROM
        tps_csv_extract_items x
    WHERE
        EXISTS (
            SELECT
                1
            FROM
                latest_extracts le
            WHERE
                le.tps_csv_extract_id = x.tps_csv_extract_id
        )
        AND result = 3
    ORDER BY
        trn;
    ```  
  - Make a note of the TRNs in order to feedback to TPS.
- Drill into records where no `establishments` record could be found which match the Local Authority Code and Establishment Number provided where *YYYYMMDD* is the year, month and day of the extract date:
  - Execute the query
    ```
    WITH
        latest_extracts AS (
            SELECT
                *
            FROM
                tps_csv_extracts
            WHERE
                filename LIKE '%YYYYMMDD%'
            ORDER BY
                created_on DESC
            LIMIT
                2
        )
    SELECT DISTINCT
        local_authority_code,
        establishment_number
    FROM
        tps_csv_extract_items x
    WHERE
        EXISTS (
            SELECT
                1
            FROM
                latest_extracts le
            WHERE
                le.tps_csv_extract_id = x.tps_csv_extract_id
        )
        AND result = 4;
    ```
  - Make a note of the LA Code / Establishment Numbers in order to feedback to TPS.
- If there are any issues with the data in the extract:
  - Add a sheet to the Excel spreadsheet **TPS Monthly Extract Issues** for the extract month.
  - Detail the specific issues.

### 5. Feedback to TPS with any issues

- Email the contact at TPS with details of any issues importing the provided files e.g.
  - List of records with any fields in an invalid format[^2].
  - List of TRNs which are not in the TRS `persons` table[^2] requesting additional personal information held in TPS for these.
  - List of Local Authority Code and Establishment Numbers which are not in the TRS `establishments` table.

[^1]: to avoid the risk of accidentally reformatting the data if editing using **Excel**.
[^2]: send the specific details via a secure channel if they contain Personal Identifiable Information (PII) and refer to that in the email.