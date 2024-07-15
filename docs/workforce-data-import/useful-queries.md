# Useful Queries

## Overview

This document contains some queries which can be used as good starting points for analysis of the workforce data.

## Queries

### Get count of records with Withdrawal Indicator 'W' that reverts in future extracts

A check for the reliability of the Withdrawal Indicator by establishments to indicate a person has left employment

```
WITH withdrawal_indicator_revert_keys AS (
	SELECT
		distinct key
	FROM
		tps_csv_extract_items x1
	WHERE
		withdrawl_indicator = 'W'
		AND EXISTS (SELECT
			   			1
			    	FROM
			   			tps_csv_extract_items x2
				    WHERE
				   		x2.key = x1.key
						AND x2.extract_date > x1.extract_date
				   		AND x2.withdrawl_indicator IS NULL)
)
SELECT
	COUNT(1)
FROM
	withdrawal_indicator_revert_keys
```

### Get examples of records with Withdrawal Indicator 'W' that reverts in future extracts

A check for the reliability of the Withdrawal Indicator by establishments to indicate a person has left employment

```
WITH withdrawal_indicator_revert_keys AS (
	SELECT
		distinct key
	FROM
		tps_csv_extract_items x1
	WHERE
		withdrawl_indicator = 'W'
		AND EXISTS (SELECT
			   			1
			    	FROM
			   			tps_csv_extract_items x2
				    WHERE
				   		x2.key = x1.key
						AND x2.extract_date > x1.extract_date
				   		AND x2.withdrawl_indicator IS NULL)
)
SELECT
	trn,
	local_authority_code,
	establishment_number,	
	employment_start_date,
	employment_end_date,
	CASE
		WHEN employment_type = 0 THEN 'FT'
		WHEN employment_type = 1 THEN 'PTR'
		WHEN employment_type = 2 THEN 'PTI'
		WHEN employment_type = 3 THEN 'PT'
	END employment_type,
	withdrawl_indicator,
	extract_date
FROM
	tps_csv_extract_items x
WHERE
	EXISTS (SELECT
		   		1
		    FROM
		   		withdrawal_indicator_revert_keys w
		    WHERE
		   		w.key = x.key)
ORDER BY
	x.key desc,
	x.extract_date
LIMIT 1000
```

### Get count of records with Withdrawal Indicator 'W' that change end date in future extracts

A check for the reliability of the Withdrawal Indicator by establishments to indicate a person has left employment

```
WITH withdrawal_end_date_change_keys AS (
	SELECT
		distinct key
	FROM
		tps_csv_extract_items x1
	WHERE
		withdrawl_indicator = 'W'
		AND EXISTS (SELECT
			   			1
			    	FROM
			   			tps_csv_extract_items x2
				    WHERE
				   		x2.key = x1.key
						AND x2.extract_date > x1.extract_date
						AND x2.withdrawl_indicator = 'W'
				   		AND x2.employment_end_date <> x1.employment_end_date)
)
SELECT
	COUNT(1)
FROM
	withdrawal_end_date_change_keys
```

### Get examples of records with Withdrawal Indicator 'W' that change end date in future extracts

A check for the reliability of the Withdrawal Indicator by establishments to indicate a person has left employment

```
WITH withdrawal_end_date_change_keys AS (
	SELECT
		distinct key
	FROM
		tps_csv_extract_items x1
	WHERE
		withdrawl_indicator = 'W'
		AND EXISTS (SELECT
			   			1
			    	FROM
			   			tps_csv_extract_items x2
				    WHERE
				   		x2.key = x1.key
						AND x2.extract_date > x1.extract_date
						AND x2.withdrawl_indicator = 'W'
				   		AND x2.employment_end_date <> x1.employment_end_date)
)
SELECT
	trn,
	local_authority_code,
	establishment_number,	
	employment_start_date,
	employment_end_date,
	CASE
		WHEN employment_type = 0 THEN 'FT'
		WHEN employment_type = 1 THEN 'PTR'
		WHEN employment_type = 2 THEN 'PTI'
		WHEN employment_type = 3 THEN 'PT'
	END employment_type,
	withdrawl_indicator,
	extract_date
FROM
	tps_csv_extract_items x
WHERE
	EXISTS (SELECT
		   		1
		    FROM
		   		withdrawal_end_date_change_keys w
		    WHERE
		   		w.key = x.key)
ORDER BY
	x.key desc,
	x.extract_date
LIMIT 1000
```

### Get count of records with Withdrawal Indicator 'W' that don't change end date or revert in future extracts

A check for the reliability of the Withdrawal Indicator by establishments to indicate a person has left employment

```
WITH withdrawal_no_change_keys AS (
	SELECT
		distinct key
	FROM
		tps_csv_extract_items x1
	WHERE
		withdrawl_indicator = 'W'
		AND EXISTS (SELECT
			   			1
			    	FROM
			   			tps_csv_extract_items x2
				    WHERE
				   		x2.key = x1.key
						AND x2.extract_date > x1.extract_date)
		AND NOT EXISTS (SELECT
							1
						FROM
							tps_csv_extract_items x2
						WHERE
							x2.key = x1.key
							AND x2.extract_date > x1.extract_date
							AND (x2.withdrawl_indicator IS NULL OR x2.employment_end_date <> x1.employment_end_date))
)
SELECT
	COUNT(1)
FROM
	withdrawal_no_change_keys
```

### Get counts of records where the end date hasn't changed for over 5 months and there is no further alternative employment

This gives an idea of people who have potentially left the teaching profession

```
WITH latest_extracts AS (
	SELECT
		*
	FROM
		(SELECT
			trn,
		 	local_authority_code,
		 	establishment_number,
		 	employment_start_date,
		 	employment_end_date,
		 	employment_type,
		 	withdrawl_indicator,
		 	extract_date,
		 	key,
		 	ROW_NUMBER() OVER (PARTITION BY key ORDER BY extract_date desc) as row_number
		 FROM
			tps_csv_extract_items) x
	WHERE
		x.row_number = 1		 	
)
SELECT
	SUM(CASE WHEN withdrawl_indicator = 'W' THEN 1 ELSE 0 END) as count_with_withdrawal_indicator,
	SUM(CASE WHEN withdrawl_indicator IS NULL THEN 1 ELSE 0 END) as count_without_withdrawal_indicator
FROM
	latest_extracts x1
WHERE
	AGE(x1.extract_date, x1.employment_end_date) > INTERVAL '5 months'
	AND NOT EXISTS (SELECT
					   		1
					    FROM
					   		latest_extracts x2
					    WHERE
					   		x2.trn = x1.trn
							AND x2.key <> x1.key
							AND AGE(x2.extract_date, x2.employment_end_date) <= INTERVAL '5 months')
```

### Get counts of records which are in the first extract and missing from multiple extracts before re-appearing in the most recent extract

This is to check that it is indeed possible that records can skip multiple extracts and later re-appear

```
WITH keys AS (
	SELECT
		*,
	    CASE 
			WHEN extract_date = to_date('20240325','YYYYMMDD') THEN 1 
			WHEN extract_date = to_date('20240425','YYYYMMDD') THEN 2
			WHEN extract_date = to_date('20240525','YYYYMMDD') THEN 3 
			WHEN extract_date = to_date('20240626','YYYYMMDD') THEN 4 
		END as extract_number
	FROM
		tps_csv_extract_items
	WHERE
		extract_date <> to_date('20240307','YYYYMMDD')
)
SELECT
	COUNT(1)
FROM
	keys k1
WHERE
	k1.extract_number = 1
	AND NOT EXISTS (SELECT
			   			1
			    	FROM
			   		    keys k2
				    WHERE
						k2.key = k1.key
						AND k2.extract_number in (2, 3))
	AND EXISTS (SELECT
			   			1
			    	FROM
			   		    keys k4
				    WHERE
						k4.key = k1.key
						AND k4.extract_number = 4)
```

### Get counts of employment types with Withdrawal Indicator 'W' 

Check that the Withrawal Indicator can also be applied to part-time employment

```
SELECT
	extract_date,
	SUM(CASE WHEN employment_type = 0 THEN 1 ELSE 0 END) full_time_count,
	SUM(CASE WHEN employment_type = 1 THEN 1 ELSE 0 END) part_time_regular_count,
	SUM(CASE WHEN employment_type = 2 THEN 1 ELSE 0 END) part_time_irregular_count,
	SUM(CASE WHEN employment_type = 3 THEN 1 ELSE 0 END) part_time_count
FROM
	tps_csv_extract_items
WHERE
	withdrawl_indicator = 'W'
GROUP BY
	extract_date
```

### Check if we are getting data with end dates within the expected date range (i.e. 6 months of the extract date) 

This is a sanity check on the data

```
SELECT
	extract_date,
	MIN(employment_end_date) min_employment_end_date,
	(date_trunc('month', extract_date) - interval '6 month')::date expected_minimum_end_date
FROM
	tps_csv_extract_items
GROUP BY
	extract_date
```

###  Get the count end dates which are beyond the end of the month of the extract date

We appear to get records where the end date is in the future

```
WITH latest_extracts AS (
	SELECT
		*,
		(date_trunc('month', extract_date) + interval '1 month' - interval '1 day')::date report_end_date
	FROM
		(SELECT
			trn,
		 	local_authority_code,
		 	establishment_number,
		 	employment_start_date,
		 	employment_end_date,
		 	employment_type,
		 	withdrawl_indicator,
		 	extract_date,
		 	key,
		 	ROW_NUMBER() OVER (PARTITION BY key ORDER BY extract_date desc) as row_number
		 FROM
			tps_csv_extract_items) x
	WHERE
		x.row_number = 1		 	
)
SELECT
	COUNT(1)
FROM
	latest_extracts
WHERE
	employment_end_date > report_end_date
```

### Get count of people who are in multiple full time employments at the same time

```
WITH overlapping_full_time AS (
	SELECT
		distinct pe1.person_id
	FROM
		person_employments pe1
	WHERE
		pe1.start_date <= pe1.last_known_employed_date
		AND pe1.employment_type = 0
		AND EXISTS (SELECT
			     		1
			        FROM
			   		    person_employments pe2
			        WHERE
						pe1.person_id = pe2.person_id
						AND pe1.person_employment_id <> pe2.person_employment_id
				   		AND pe2.employment_type = 0
						AND pe2.start_date <= pe2.last_known_employed_date
				   		AND daterange(pe1.start_date, pe1.last_known_employed_date,'[]') && daterange(pe2.start_date, pe2.last_known_employed_date,'[]'))
)
SELECT
	COUNT(1)
FROM
	overlapping_full_time
```

### Examples of people who are in multiple full time employments at the same time

```
WITH overlapping_full_time AS (
	SELECT
		distinct pe1.person_id
	FROM
		person_employments pe1
	WHERE
		pe1.start_date <= pe1.last_known_employed_date
		AND pe1.employment_type = 0
		AND EXISTS (SELECT
			     		1
			        FROM
			   		    person_employments pe2
			        WHERE
						pe1.person_id = pe2.person_id
						AND pe1.person_employment_id <> pe2.person_employment_id
				   		AND pe2.employment_type = 0
						AND pe2.start_date <= pe2.last_known_employed_date
				   		AND daterange(pe1.start_date, pe1.last_known_employed_date,'[]') && daterange(pe2.start_date, pe2.last_known_employed_date,'[]'))
)
SELECT
	p.trn,
    e.la_code,
    e.establishment_number,
	substr(e.establishment_name, 1, 30),
	pe.start_date,
	pe.last_known_employed_date	
FROM
		person_employments pe
	JOIN
		persons p ON p.person_id = pe.person_id
	JOIN
		establishments e ON e.establishment_id = pe.establishment_id
WHERE
	pe.employment_type = 0
	AND EXISTS (SELECT
		   		1
		    FROM
		   		overlapping_full_time o
		    WHERE
		   		o.person_id = pe.person_id)
ORDER BY
	p.trn,
	start_date
```

### Get most frequent end dates for extract

Check which are the most frequent end dates for a given extract

```
SELECT
	employment_end_date,
	COUNT(1)
FROM
	tps_csv_extract_items
WHERE
	extract_date = to_date('20240626','YYYYMMDD')
GROUP BY
	employment_end_date
ORDER BY 
	COUNT(1) DESC
LIMIT 10
```

### Get count of records which change employment type between extracts

```
WITH updated_employment_type_keys AS (
	SELECT
		distinct key
	FROM
		tps_csv_extract_items x1
	WHERE		
		EXISTS (SELECT
			   			1
			    	FROM
			   			tps_csv_extract_items x2
				    WHERE
				   		x2.key = x1.key
						AND x2.employment_type <> x1.employment_type
						AND x2.extract_date > x1.extract_date)
)
SELECT
	COUNT(key)
FROM
	updated_employment_type_keys
```

### Get example records which change employment type between extracts

```
WITH updated_employment_type_keys AS (
	SELECT
		distinct key
	FROM
		tps_csv_extract_items x1
	WHERE
		EXISTS (SELECT
			   			1
			    	FROM
			   			tps_csv_extract_items x2
				    WHERE
				   		x2.key = x1.key
						AND x2.employment_type <> x1.employment_type
						AND x2.extract_date > x1.extract_date)
)
SELECT
	trn,
	local_authority_code,
	establishment_number,	
	employment_start_date,
	employment_end_date,
	CASE
		WHEN employment_type = 0 THEN 'FT'
		WHEN employment_type = 1 THEN 'PTR'
		WHEN employment_type = 2 THEN 'PTI'
		WHEN employment_type = 3 THEN 'PT'
	END employment_type,
	withdrawl_indicator,
	extract_date
FROM
	tps_csv_extract_items x
WHERE
	EXISTS (SELECT
		   		1
		    FROM
		   		updated_employment_type_keys e
		    WHERE
		   		e.key = x.key)
ORDER BY
	x.key desc,
	x.extract_date
LIMIT 1000
```

### Get all TPS establishments not in the Establishment Codes PDF ranges

```
WITH unique_gias_establishments AS (
    SELECT
        establishment_id,
        la_code,
        la_name,
        establishment_number,
        establishment_name,
        establishment_type_code,
        postcode
    FROM
        (SELECT
            establishment_id,
            la_code,
            la_name,
            establishment_number,
            establishment_name,
            establishment_type_code,
            postcode,
            ROW_NUMBER() OVER (PARTITION BY la_code, establishment_number, CASE WHEN establishment_number IS NULL THEN postcode ELSE NULL END ORDER BY translate(establishment_status_code::text, '1234', '1324'), urn desc) as row_number
        FROM
            establishments
        WHERE
            establishment_source_id = 1) e
    WHERE
        e.row_number = 1
),
unique_tps_establishments AS (
    SELECT
        tps_establishment_id,
        la_code,
        establishment_code,
        employers_name,
        school_gias_name,
        school_closed_date
    FROM
        (SELECT
            tps_establishment_id,
            la_code,
            establishment_code,
            employers_name,
            school_gias_name,
            school_closed_date,
            ROW_NUMBER() OVER (PARTITION BY la_code, establishment_code ORDER BY CASE WHEN school_closed_date IS NULL THEN 1 ELSE 2 END) as row_number
         FROM
            tps_establishments) e
    WHERE
        e.row_number = 1
)
SELECT
	*
FROM
	unique_tps_establishments e
WHERE
	NOT EXISTS (SELECT
					1
				FROM
					unique_gias_establishments g
				WHERE
					g.la_code = e.la_code
					AND g.establishment_number = e.establishment_code)
	AND NOT EXISTS (SELECT
				   		1
				    FROM
				   		tps_establishment_types t
					WHERE
						e.establishment_code::int >= t.establishment_range_from::int
                        AND e.establishment_code::int <= t.establishment_range_to::int)
ORDER BY
	la_code,
	establishment_code
```